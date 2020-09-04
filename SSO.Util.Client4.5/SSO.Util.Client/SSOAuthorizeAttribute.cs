using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SSO.Util.Client
{
    /// <summary>
    /// sso验证
    /// </summary>
    public class SSOAuthorizeAttribute : AuthorizeAttribute
    {
        public static string BaseUrl = AppSettings.GetValue("ssoBaseUrl");
        public static string SecretKey = AppSettings.GetValue("ssoSecretKey");
        public static string CookieKey = AppSettings.GetValue("ssoCookieKey");
        public static string CookieTime = AppSettings.GetValue("ssoCookieTime");
        public bool UnAuthorizedRedirect = true;
        /// <summary>
        /// 验证不通过是否跳转到sso登录页面
        /// </summary>
        /// <param name="unAuthorizedRedirect">验证不通过是否跳转到sso登录页面</param>
        public SSOAuthorizeAttribute(bool unAuthorizedRedirect = true)
        {
            UnAuthorizedRedirect = unAuthorizedRedirect;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var reflectedActionDescriptor = (ReflectedActionDescriptor)filterContext.ActionDescriptor;
            IEnumerable<CustomAttributeData> methodAttributes = reflectedActionDescriptor.MethodInfo.CustomAttributes;
            IEnumerable<CustomAttributeData> controllerAttributes = reflectedActionDescriptor.ControllerDescriptor.ControllerType.CustomAttributes;
            bool isAuthorization = true;
            List<string> roles = new List<string>();
            foreach (CustomAttributeData item in controllerAttributes)
            {
                if (item.AttributeType.Name == "AllowAnonymousAttribute") isAuthorization = false;
                if (item.AttributeType.Name == "SSOAuthorizeAttribute") isAuthorization = true;
                foreach (var it in item.NamedArguments)
                {
                    if (it.MemberName != "Roles") continue;
                    roles.AddRange(it.TypedValue.Value.ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }
            }
            foreach (CustomAttributeData item in methodAttributes)
            {
                if (item.AttributeType.Name == "AllowAnonymousAttribute") isAuthorization = false;
                if (item.AttributeType.Name == "SSOAuthorizeAttribute") isAuthorization = true;
                foreach (var it in item.NamedArguments)
                {
                    if (it.MemberName != "Roles") continue;
                    roles.AddRange(it.TypedValue.Value.ToString().Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                }
            }
            if (!isAuthorization) return;
            //验证配置文件
            if (!VerifyConfig(filterContext)) return;
            HttpRequestBase request = filterContext.HttpContext.Request;
            var ssourl = request.QueryString["ssourls"];
            var absoluteUrl = AppSettings.GetAbsoluteUri(request);
            if (!string.IsNullOrEmpty(ssourl)) //sso 退出
            {
                var returnUrl = request.QueryString["returnUrl"];
                ////////清除本站cookie
                List<string> ssoUrls = JsonSerializerHelper.Deserialize<List<string>>(Encoding.UTF8.GetString(Convert.FromBase64String(Base64SecureURL.Decode(ssourl))));
                var cookie = request.Cookies[CookieKey];
                if (cookie != null)
                {
                    cookie.Expires = DateTime.Now.AddYears(-1);
                    filterContext.HttpContext.Response.Cookies.Add(cookie);
                }
                /////////////////////
                for (var i = 0; i < ssoUrls.Count; i++)
                {
                    if (absoluteUrl.Contains(ssoUrls[i]))
                    {
                        ssoUrls.RemoveAt(i);
                        break;
                    }
                }
                if (ssoUrls.Count > 0)
                {
                    string newSsoUrls = JsonSerializerHelper.Serialize(ssoUrls);
                    filterContext.Result = new RedirectResult(ssoUrls[0] + "?ssourls=" + newSsoUrls.StrToBase64() + "&returnUrl=" + returnUrl);
                }
                else //最后一个
                {
                    filterContext.Result = new RedirectResult(BaseUrl + "?returnUrl=" + returnUrl);
                }
                return;
            }
            string authorization = JwtManager.GetAuthorization(request, CookieKey);
            string ticket = request.QueryString["ticket"];
            if (string.IsNullOrEmpty(authorization))
            {
                if (string.IsNullOrEmpty(ticket))
                {
                    filterContext.Result = GetActionResult(absoluteUrl);
                    return;
                }
                else
                {
                    string from = AppSettings.GetApplicationUrl(request).ReplaceHttpPrefix().TrimEnd('/');
                    authorization = GetTokenByTicket(from, ticket, request.UserHostAddress);
                    if (!string.IsNullOrEmpty(authorization))
                    {
                        HttpCookie httpCookie = new HttpCookie(CookieKey, authorization);
                        if (CookieTime != "session")
                        {
                            httpCookie.Expires = DateTime.Now.AddMinutes(Convert.ToInt32(CookieTime));
                        }
                        filterContext.HttpContext.Response.Cookies.Add(httpCookie);
                    }
                    else
                    {
                        filterContext.Result = GetActionResult(absoluteUrl);
                        return;
                    }
                }
            }
            try
            {
                var principal = JwtManager.ParseAuthorization(authorization, SecretKey);
                filterContext.HttpContext.User = principal;
                if (!CheckRole(roles, authorization)) filterContext.Result = new ResponseModel<string>(ErrorCode.authorize_fault, "");
            }
            catch (Exception ex) //token失效
            {
                Log4Net.ErrorLog(ex);
                HttpCookie httpCookie = filterContext.HttpContext.Request.Cookies[CookieKey];
                if (httpCookie != null)
                {
                    httpCookie.Expires = DateTime.Now.AddYears(-1);
                    filterContext.HttpContext.Response.Cookies.Add(httpCookie);
                }
                filterContext.Result = GetActionResult(absoluteUrl);
            }
        }
        private ActionResult GetActionResult(string returnUrl)
        {
            ActionResult result = new ResponseModel<string>(ErrorCode.authorize_fault, "");
            if (UnAuthorizedRedirect) result = new RedirectResult(BaseUrl.TrimEnd('/') + "/sso/login" + "?returnUrl=" + returnUrl);
            return result;
        }
        private bool CheckRole(IEnumerable<string> roles, string authorization)
        {
            if (roles.Count() == 0) return true;
            //数据库中的role
            string[] dataRoles = GetRoles(authorization);
            //如果有交集,可以访问
            if (roles.Intersect(dataRoles).Count() > 0) return true;
            return false;
        }
        /// <summary>
        /// 根据url上面的ticket获取token
        /// </summary>
        /// <param name="from"></param>
        /// <param name="ticket"></param>
        /// <param name="audience"></param>
        /// <returns></returns>
        public static string GetTokenByTicket(string from, string ticket, string audience)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BaseUrl.TrimEnd('/') + "/sso/gettoken" + "?from=" + from + "&ticket=" + ticket + "&ip=" + audience);
            request.Method = "get";
            using (WebResponse response = request.GetResponse())
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string resp = reader.ReadToEnd();
                var result = JsonSerializerHelper.Deserialize<ServiceModel<string>>(resp);
                if (result.code == 0) return result.result;
                return "";
            }
        }
        /// <summary>
        /// 根据token获取roles列表
        /// </summary>
        /// <param name="authorization"></param>
        /// <returns></returns>
        public static string[] GetRoles(string authorization)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BaseUrl.TrimEnd('/') + "/user/getroles");
            request.Method = "get";
            request.Headers.Add("Authorization", authorization);
            using (WebResponse response = request.GetResponse())
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string resp = reader.ReadToEnd();
                var result = JsonSerializerHelper.Deserialize<ServiceModel<string[]>>(resp);
                if (result.code == 0) return result.result;
                return new string[] { };
            }
        }
        /// <summary>
        /// 验证配置文件
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        public bool VerifyConfig(AuthorizationContext filterContext)
        {
            if (BaseUrl.IsNullOrEmpty())
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.baseUrl_not_config, "");
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
            if (CookieTime.IsNullOrEmpty())
            {
                filterContext.Result = new ResponseModel<string>(ErrorCode.cookieTime_not_config, "");
                return false;
            }
            return true;
        }
    }
}
