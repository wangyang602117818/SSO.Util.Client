using System;
using System.Collections.Generic;
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
    public class LogRecordAttribute : ActionFilterAttribute
    {
        public static string messageBaseUrl = AppSettings.GetValue("messageBaseUrl");
        public bool RecordQuerystring = true;
        /// <summary>
        /// 是否记录请求体
        /// </summary>
        public bool RecordContent = true;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var reflectedActionDescriptor = (ReflectedActionDescriptor)filterContext.ActionDescriptor;
            IEnumerable<CustomAttributeData> methodAttributes = reflectedActionDescriptor.MethodInfo.CustomAttributes;
            var controllerAttributes = reflectedActionDescriptor.ControllerDescriptor.GetCustomAttributes(true);
            //是否记录日志标记
            bool isLog = true;
            foreach (var item in controllerAttributes)
            {
                if (item.GetType().Name == "NoneLogRecordAttribute") isLog = false;
                if (item.GetType().Name == "LogRecordAttribute") isLog = true;
            }
            foreach (CustomAttributeData c in methodAttributes)
            {
                if (c.AttributeType.Name == "NoneLogRecordAttribute") isLog = false;
                if (c.AttributeType.Name == "LogRecordAttribute") isLog = true;
            }
            if (!isLog) return;
            if (messageBaseUrl.IsNullOrEmpty())
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.logBaseUrl_not_config, "");
                return;
            }
            MessageCenterService messageService = new MessageCenterService(messageBaseUrl);
            HttpRequestBase request = filterContext.HttpContext.Request;
            //日志来源
            var from = request.Url.Scheme + "://" + request.Url.Host + ":" + request.Url.Port + request.ApplicationPath;
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
                Stream sm = request.InputStream;
                sm.Position = 0;
                content = (new StreamReader(sm)).ReadToEnd().Replace("\n", "").Replace("\t", "");
                sm.Position = 0;
            }
            string userId = "", userName = "";
            string authorization = SSOAuthorizeAttribute.GetAuthorization(request);
            if (!authorization.IsNullOrEmpty())
            {
                ClaimsPrincipal claimsPrincipal = SSOAuthorizeAttribute.ParseAuthorization(authorization);
                UserData userData = SSOAuthorizeAttribute.ParseUserData(claimsPrincipal);
                userId = userData.UserId;
                userName = userData.UserName;
            }
            string userHost = request.UserHostAddress;
            string userAgent = request.UserAgent;
            messageService.InsertLog(from.ReplaceHttpPrefix().TrimEnd('/'), controller, action, route, querystring, content, userId, userName, userHost, userAgent);
            base.OnActionExecuting(filterContext);
        }
    }
}
