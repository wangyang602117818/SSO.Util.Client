using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SSO.Util.Client
{
    /// <summary>
    /// 过滤器发生在modelbinding之后,所以如果model验证不通过,则不会记录日志
    /// </summary>
    public class LogRecordAttribute : ActionFilterAttribute
    {
        private static string messageBaseUrl = AppSettings.GetValue("messageBaseUrl");
        private static string cookieKey = AppSettings.GetValue("ssoCookieKey");
        public static string secretKey = AppSettings.GetValue("ssoSecretKey");
        public bool RecordQuerystring = true;
        /// <summary>
        /// 是否记录请求体
        /// </summary>
        public bool RecordContent = true;
        public LogRecordAttribute(bool recordQuerystring = true, bool recordContent = true)
        {
            RecordQuerystring = recordQuerystring;
            RecordContent = recordContent;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //验证配置文件
            if (!VerifyConfig(filterContext)) return;
            filterContext.HttpContext.Items.Add("log_time_start", DateTime.UtcNow.UTCMillisecondTimeStamp());
            base.OnActionExecuting(filterContext);
        }
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var reflectedActionDescriptor = (ReflectedActionDescriptor)filterContext.ActionDescriptor;
            IEnumerable<CustomAttributeData> methodAttributes = reflectedActionDescriptor.MethodInfo.CustomAttributes;
            IEnumerable<CustomAttributeData> controllerAttributes = reflectedActionDescriptor.ControllerDescriptor.ControllerType.CustomAttributes;
            //是否记录日志标记
            bool isLog = true;
            foreach (CustomAttributeData c in controllerAttributes)
            {
                if (c.AttributeType.Name == "NoneLogRecordAttribute") isLog = false;
                if (c.AttributeType.Name == "LogRecordAttribute") isLog = true;
            }
            foreach (CustomAttributeData c in methodAttributes)
            {
                if (c.AttributeType.Name == "NoneLogRecordAttribute") isLog = false;
                if (c.AttributeType.Name == "LogRecordAttribute") isLog = true;
            }
            if (!isLog) return;
            MessageCenterService messageService = new MessageCenterService(messageBaseUrl);
            HttpRequestBase request = filterContext.HttpContext.Request;
            //日志来源
            var from = AppSettings.GetApplicationUrl(request);
            //不使用路由中的字符串因为用户可能输入大小写,不利于统计
            var controller = reflectedActionDescriptor.ControllerDescriptor.ControllerName;
            var action = reflectedActionDescriptor.MethodInfo.Name;
            //路由,解决 home/index/1 后面的1无法记录
            var route = "";
            foreach (var item in filterContext.RouteData.Values)
            {
                var value = item.Value.ToString().ToLower();
                if (value == controller.ToLower() || value == action.ToLower()) continue;
                route += item.Key + "=" + item.Value.ToString() + "&";
            }
            route = route.TrimEnd('&');
            var querystring = "*";
            if (RecordQuerystring) querystring = request.Url.Query;
            var content = "*";
            if (RecordContent)
            {
                var files = request.Files;
                if (files.Count > 0)
                {
                    List<string> fileNames = new List<string>();
                    for (var i = 0; i < files.Count; i++)
                        fileNames.Add(files[i].FileName);
                    content = string.Join(",", fileNames);
                }
                else
                {
                    Stream sm = request.InputStream;
                    sm.Position = 0;
                    content = (new StreamReader(sm)).ReadToEnd().Replace("\n", "").Replace("\t", "").Replace("\r", "");
                    sm.Position = 0;
                }
            }
            string userId = "", userName = "";
            string authorization = JwtManager.GetAuthorization(request, cookieKey);
            if (!authorization.IsNullOrEmpty())
            {
                ClaimsPrincipal claimsPrincipal = JwtManager.ParseAuthorization(authorization, secretKey);
                UserData userData = JwtManager.ParseUserData(claimsPrincipal);
                userId = userData.UserId;
                userName = userData.UserName;
            }
            string userHost = request.UserHostAddress;
            string userAgent = request.UserAgent;
            var time = DateTime.UtcNow.UTCMillisecondTimeStamp() - (long)filterContext.HttpContext.Items["log_time_start"];
            bool exception = filterContext.Exception != null;
            messageService.InsertLog(from.ReplaceHttpPrefix().TrimEnd('/'), controller, action, route, querystring, content, userId, userName, userHost, userAgent, time, exception);
            base.OnActionExecuted(filterContext);
        }
        public bool VerifyConfig(ActionExecutingContext filterContext)
        {
            if (messageBaseUrl.IsNullOrEmpty())
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.messageBaseUrl_not_config, "");
                return false;
            }
            if (secretKey.IsNullOrEmpty())
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.secretKey_not_config, "");
                return false;
            }
            if (cookieKey.IsNullOrEmpty())
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.cookieKey_not_config, "");
                return false;
            }
            return true;
        }
    }
}
