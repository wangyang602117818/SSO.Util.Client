using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SSO.Util.Client
{
    public class LogActionFilterAttribute: ActionFilterAttribute
    {
        public static string logBaseUrl = AppSettings.GetValue("logBaseUrl");
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (logBaseUrl.IsNullOrEmpty())
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.logBaseUrl_not_config, "");
                return;
            }
            LogCenterService logService = new LogCenterService(logBaseUrl);
            HttpRequestBase request = filterContext.HttpContext.Request;
            //日志来源
            var from = request.Url.Scheme + "://" + request.Url.Host + ":" + request.Url.Port + request.ApplicationPath;
            var controller = filterContext.RouteData.Values["controller"].ToString();
            var action = filterContext.RouteData.Values["action"].ToString();
            var querystring = request.Url.Query;
            Stream sm = request.InputStream;
            sm.Position = 0;
            var content = (new StreamReader(sm)).ReadToEnd();
            sm.Position = 0;
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
            logService.Insert(from, controller, action, querystring, content, userId, userName, userHost, userAgent);
            base.OnActionExecuting(filterContext);
        }
    }
}
