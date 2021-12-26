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
        public static string BaseUrl = AppSettings.GetValue("messageBaseUrl");
        public static string CookieKey = AppSettings.GetValue("ssoCookieKey");
        public static string SecretKey = AppSettings.GetValue("ssoSecretKey");
        /// <summary>
        /// 是否记录querystring
        /// </summary>
        public bool RecordQuerystring = true;
        /// <summary>
        /// 是否记录请求体
        /// </summary>
        public bool RecordRequestContent = true;
        /// <summary>
        /// 是否记录响应体
        /// </summary>
        public bool RecordResponseContent = true;
        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="recordQuerystring"></param>
        /// <param name="recordRequestContent"></param>
        /// <param name="recordResponseContent"></param>
        public LogRecordAttribute(bool recordQuerystring = true, bool recordRequestContent = true, bool recordResponseContent = true)
        {
            RecordQuerystring = recordQuerystring;
            RecordRequestContent = recordRequestContent;
            RecordResponseContent = recordResponseContent;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var reflectedActionDescriptor = (ReflectedActionDescriptor)filterContext.ActionDescriptor;
            IEnumerable<CustomAttributeData> methodAttributes = reflectedActionDescriptor.MethodInfo.CustomAttributes;
            IEnumerable<CustomAttributeData> controllerAttributes = reflectedActionDescriptor.ControllerDescriptor.ControllerType.CustomAttributes;
            bool logRecord = CheckLogRecord(methodAttributes, controllerAttributes);
            if (!VerifyConfig(filterContext, logRecord)) return;
            filterContext.HttpContext.Items.Add("log_time_start", DateTime.UtcNow.MillisecondTimeStamp());
            base.OnActionExecuting(filterContext);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var reflectedActionDescriptor = (ReflectedActionDescriptor)filterContext.ActionDescriptor;
            IEnumerable<CustomAttributeData> methodAttributes = reflectedActionDescriptor.MethodInfo.CustomAttributes;
            IEnumerable<CustomAttributeData> controllerAttributes = reflectedActionDescriptor.ControllerDescriptor.ControllerType.CustomAttributes;
            if (!CheckLogRecord(methodAttributes, controllerAttributes)) return;
            MessageCenterService messageService = new MessageCenterService(BaseUrl);
            HttpRequestBase request = filterContext.HttpContext.Request;
            //日志调用api
            var to = AppSettings.GetApplicationUrl(request).ReplaceHttpPrefix().TrimEnd('/').ToLower();
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
            var requestContent = "*";
            if (RecordRequestContent)
            {
                var files = request.Files;
                if (files.Count > 0)
                {
                    List<string> fileNames = new List<string>();
                    for (var i = 0; i < files.Count; i++)
                        fileNames.Add(files[i].FileName);
                    requestContent = string.Join(",", fileNames);
                }
                else
                {
                    Stream sm = request.InputStream;
                    sm.Position = 0;
                    requestContent = (new StreamReader(sm)).ReadToEnd().Replace("\n", "").Replace("\t", "").Replace("\r", "");
                    sm.Position = 0;
                }
            }
            var responseContent = "*";
            if (RecordResponseContent)
            {
                var result = filterContext.Result;
                if (result is JsonResult) responseContent = JsonSerializerHelper.Serialize(((JsonResult)result).Data);
                if (result is ViewResult) responseContent = "ViewResult";
                if (result is ContentResult) responseContent = ((ContentResult)result).Content;
                if (result is HttpStatusCodeResult) responseContent = ((HttpStatusCodeResult)result).StatusCode + "-" + ((HttpStatusCodeResult)result).StatusDescription;
                if (result is FileResult) responseContent = ((FileResult)result).FileDownloadName;
                if (result is JavaScriptResult) responseContent = ((JavaScriptResult)result).Script;
                if (result is EmptyResult) responseContent = "";
                if (result is RedirectResult) responseContent = "redirect:" + ((RedirectResult)result).Url;
                if (result is RedirectToRouteResult) responseContent = "route:" + ((RedirectToRouteResult)result).RouteName;
            }
            string userId = "", userName = "", from = "";
            string authorization = JwtManager.GetAuthorization(CookieKey);
            if (!authorization.IsNullOrEmpty())
            {
                ClaimsPrincipal claimsPrincipal = JwtManager.ParseAuthorization(authorization, SecretKey);
                UserData userData = JwtManager.ParseUserData(claimsPrincipal);
                userId = userData.UserId;
                userName = userData.UserName;
                from = userData.From.ReplaceHttpPrefix().TrimEnd('/').ToLower();
            }
            string userHost = request.UserHostAddress;
            string userAgent = request.UserAgent;
            var time = DateTime.UtcNow.MillisecondTimeStamp() - (long)filterContext.HttpContext.Items["log_time_start"];
            bool exception = filterContext.Exception != null;
            messageService.InsertLog(from, to, controller, action, route, querystring, requestContent, responseContent, userId, userName, userHost, userAgent, time, exception);
            base.OnActionExecuted(filterContext);
        }
        /// <summary>
        /// 验证配置文件
        /// </summary>
        /// <param name="filterContext"></param>
        /// <param name="logRecord">是否记录日志</param>
        /// <returns></returns>
        public bool VerifyConfig(ActionExecutingContext filterContext, bool logRecord)
        {
            if (BaseUrl.IsNullOrEmpty() && logRecord)
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.messageBaseUrl_not_config, "");
                return false;
            }
            if (SecretKey.IsNullOrEmpty() && logRecord)
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.secretKey_not_config, "");
                return false;
            }
            if (CookieKey.IsNullOrEmpty() && logRecord)
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.cookieKey_not_config, "");
                return false;
            }
            return true;
        }
        /// <summary>
        /// 判断是否记录日志
        /// </summary>
        /// <param name="methodAttributes"></param>
        /// <param name="controllerAttributes"></param>
        /// <returns></returns>
        private bool CheckLogRecord(IEnumerable<CustomAttributeData> methodAttributes, IEnumerable<CustomAttributeData> controllerAttributes)
        {
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
            return isLog;
        }
    }
}
