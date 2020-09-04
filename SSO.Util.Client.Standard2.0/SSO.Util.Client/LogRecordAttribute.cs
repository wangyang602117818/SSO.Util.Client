using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SSO.Util.Client
{
    /// <summary>
    /// .net core 中request.Body默认读取一次就销毁了,所以该过滤器一定要在model binding之前执行,
    /// </summary>
    public class LogRecordAttribute : Attribute, IActionFilter, IResourceFilter
    {
        public static string BaseUrl = AppSettings.GetValue("messageBaseUrl");
        public static string CookieKey = AppSettings.GetValue("ssoCookieKey");
        public static string SecretKey = AppSettings.GetValue("ssoSecretKey");
        /// <summary>
        /// 是否记录url查询
        /// </summary>
        public bool RecordQuerystring = true;
        /// <summary>
        /// 是否记录请求体
        /// </summary>
        public bool RecordContent = true;
        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="recordQuerystring"></param>
        /// <param name="recordContent"></param>
        public LogRecordAttribute(bool recordQuerystring = true, bool recordContent = true)
        {
            RecordQuerystring = recordQuerystring;
            RecordContent = recordContent;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            context.HttpContext.Request.EnableBuffering();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!VerifyConfig(context)) return;
            context.HttpContext.Items.Add("log_time_start", DateTime.UtcNow.UTCMillisecondTimeStamp());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
            var actionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
            IEnumerable<CustomAttributeData> methodAttributes = actionDescriptor.MethodInfo.CustomAttributes;
            IEnumerable<CustomAttributeData> controllerAttributes = actionDescriptor.ControllerTypeInfo.CustomAttributes;
            //是否记录日志标记
            bool isLog = true;
            foreach (CustomAttributeData item in controllerAttributes)
            {
                if (item.AttributeType.Name == "NoneLogRecordAttribute") isLog = false;
                if (item.AttributeType.Name == "LogRecordAttribute") isLog = true;
            }
            foreach (CustomAttributeData c in methodAttributes)
            {
                if (c.AttributeType.Name == "NoneLogRecordAttribute") isLog = false;
                if (c.AttributeType.Name == "LogRecordAttribute") isLog = true;
            }
            if (!isLog) return;
            MessageCenterService messageService = new MessageCenterService(BaseUrl);
            HttpRequest request = context.HttpContext.Request;
            var from = AppSettings.GetApplicationUrl(request);
            //不使用路由中的字符串因为用户可能输入大小写,不利于统计
            var controller = actionDescriptor.ControllerName;
            var action = actionDescriptor.ActionName;
            //路由,解决 home/index/1 后面的1无法记录
            var route = "";
            foreach (var item in context.RouteData.Values)
            {
                var value = item.Value.ToString().ToLower();
                if (value == controller.ToLower() || value == action.ToLower()) continue;
                route += item.Key + "=" + item.Value.ToString() + "&";
            }
            route = route.TrimEnd('&');
            var querystring = "*";
            if (RecordQuerystring) querystring = request.QueryString.Value;
            var content = "*";
            if (RecordContent)
            {
                var hasForm = request.HasFormContentType;
                if (hasForm && request.Form.Files.Count > 0)
                {
                    List<string> fileNames = new List<string>();
                    for (var i = 0; i < request.Form.Files.Count; i++)
                        fileNames.Add(request.Form.Files[i].FileName);
                    content = string.Join(",", fileNames);
                }
                else
                {
                    request.Body.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(request.Body);
                    content = reader.ReadToEndAsync().Result.Replace("\n", "").Replace("\t", "").Replace("\r", "");
                    request.Body.Seek(0, SeekOrigin.Begin);
                }
            }
            string userId = "", userName = "";
            string authorization = JwtManager.GetAuthorization(request, CookieKey);
            if (!authorization.IsNullOrEmpty())
            {
                ClaimsPrincipal claimsPrincipal = JwtManager.ParseAuthorization(authorization, SecretKey, request.HttpContext);
                UserData userData = JwtManager.ParseUserData(claimsPrincipal);
                userId = userData.UserId;
                userName = userData.UserName;
            }
            string userHost = request.HttpContext.Connection.RemoteIpAddress.ToString();
            string userAgent = request.Headers["User-Agent"];
            var time = DateTime.UtcNow.UTCMillisecondTimeStamp() - (long)context.HttpContext.Items["log_time_start"];
            bool exception = context.Exception != null;
            messageService.InsertLog(from.ReplaceHttpPrefix().TrimEnd('/').ToLower(), controller, action, route, querystring, content, userId, userName, userHost, userAgent, time, exception);
        }
        /// <summary>
        /// 验证配置文件
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        public bool VerifyConfig(ActionExecutingContext filterContext)
        {
            if (BaseUrl.IsNullOrEmpty())
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.messageBaseUrl_not_config, "");
                return false;
            }
            if (SecretKey.IsNullOrEmpty())
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.secretKey_not_config, "");
                return false;
            }
            if (CookieKey.IsNullOrEmpty())
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.cookieKey_not_config, "");
                return false;
            }
            return true;
        }
    }
}
