using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;

namespace SSO.Util.Client
{
    public class SSOAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public string BaseUrl = AppSettings.GetValue("ssoBaseUrl");
        public string SecretKey = AppSettings.GetValue("ssoSecretKey");
        public string CookieKey = AppSettings.GetValue("ssoCookieKey");
        public string CookieTime = AppSettings.GetValue("ssoCookieTime");
        public string Roles { get; set; }
        public bool UnAuthorizedRedirect = true;
        public SSOAuthorizeAttribute(bool unAuthorizedRedirect = true)
        {
            UnAuthorizedRedirect = unAuthorizedRedirect;
        }
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            var actionDescriptor = (ControllerActionDescriptor)filterContext.ActionDescriptor;
            IEnumerable<CustomAttributeData> methodAttributes = actionDescriptor.MethodInfo.CustomAttributes;
            IEnumerable<CustomAttributeData> controllerAttributes = actionDescriptor.ControllerTypeInfo.CustomAttributes;
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
            HttpRequest request = filterContext.HttpContext.Request;
            var ssourl = request.Query["ssourls"];
            var absoluteUrl = AppSettings.GetAbsoluteUri(request);
            if (!string.IsNullOrEmpty(ssourl)) //sso 退出
            {
                var returnUrl = request.Query["returnUrl"];
                ////////清除本站cookie
                List<string> ssoUrls = JsonSerializerHelper.Deserialize<List<string>>(Encoding.UTF8.GetString(Convert.FromBase64String(Base64SecureURL.Decode(ssourl))));
                var cookie = request.Cookies[CookieKey];
                if (cookie != null)
                {
                    filterContext.HttpContext.Response.Cookies.Delete(CookieKey);
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
            string ticket = request.Query["ticket"];
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
                    authorization = GetTokenByTicket(from, ticket, request.HttpContext.Connection.RemoteIpAddress.ToString());
                    if (!string.IsNullOrEmpty(authorization))
                    {
                        if (CookieTime != "session")
                        {
                            filterContext.HttpContext.Response.Cookies.Append(CookieKey, authorization, new CookieOptions()
                            {
                                Expires = DateTime.Now.AddMinutes(Convert.ToInt32(CookieTime))
                            });
                        }
                        else
                        {
                            filterContext.HttpContext.Response.Cookies.Append(CookieKey, authorization);
                        }
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
                var principal = JwtManager.ParseAuthorization(authorization, SecretKey, filterContext.HttpContext);
                filterContext.HttpContext.User = principal;
                if (!CheckRole(roles, authorization)) filterContext.Result = new ResponseModel<string>(ErrorCode.authorize_fault, "");
            }
            catch (Exception ex) //token失效
            {
                Log4Net.ErrorLog(ex);
                var httpCookie = filterContext.HttpContext.Request.Cookies[CookieKey];
                if (httpCookie != null)
                {
                    filterContext.HttpContext.Response.Cookies.Delete(CookieKey);
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
        public string GetTokenByTicket(string from, string ticket, string audience)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(BaseUrl.TrimEnd('/') + "/sso/gettoken?from=" + from + "&ticket=" + ticket + "&ip=" + audience);
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
        public string[] GetRoles(string authorization)
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
        public bool VerifyConfig(AuthorizationFilterContext filterContext)
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
