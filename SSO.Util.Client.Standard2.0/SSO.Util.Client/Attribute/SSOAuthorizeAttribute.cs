﻿using Microsoft.AspNetCore.Authorization;
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
    /// <summary>
    /// sso验证
    /// </summary>
    public class SSOAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public static string BaseUrl = AppSettings.GetValue("ssoBaseUrl");
        public static string SecretKey = AppSettings.GetValue("ssoSecretKey");
        public static string CookieKey = AppSettings.GetValue("ssoCookieKey");
        public static string CookieTime = AppSettings.GetValue("ssoCookieTime");
        public bool UnAuthorizedRedirect = true;
        public string Name = "";
        /// <summary>
        /// 验证不通过是否跳转到sso登录页面
        /// </summary>
        /// <param name="name">权限名称,到数据库查询是否有权限</param>
        /// <param name="unAuthorizedRedirect">验证不通过是否跳转到sso登录页面</param>
        public SSOAuthorizeAttribute(string name = "", bool unAuthorizedRedirect = true)
        {
            Name = name;
            UnAuthorizedRedirect = unAuthorizedRedirect;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filterContext"></param>
        public void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            var actionDescriptor = (ControllerActionDescriptor)filterContext.ActionDescriptor;
            IEnumerable<CustomAttributeData> methodAttributes = actionDescriptor.MethodInfo.CustomAttributes;
            IEnumerable<CustomAttributeData> controllerAttributes = actionDescriptor.ControllerTypeInfo.CustomAttributes;
            bool isAuthorization = true;
            string permissionName = "";
            foreach (CustomAttributeData item in controllerAttributes)
            {
                if (item.AttributeType.Name == "AllowAnonymousAttribute") isAuthorization = false;
                if (item.AttributeType.Name == "SSOAuthorizeAttribute")
                {
                    isAuthorization = true;
                    if (item.ConstructorArguments.Count > 0)
                    {
                        permissionName = item.ConstructorArguments[0].Value.ToString();
                    }
                }
            }
            foreach (CustomAttributeData item in methodAttributes)
            {
                if (item.AttributeType.Name == "AllowAnonymousAttribute") isAuthorization = false;
                if (item.AttributeType.Name == "SSOAuthorizeAttribute")
                {
                    isAuthorization = true;
                    if (item.ConstructorArguments.Count > 0)
                    {
                        permissionName = item.ConstructorArguments[0].Value.ToString();
                    }
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
                    filterContext.Result = new RedirectResult(BaseUrl.TrimEnd('/') + "/sso/login?returnUrl=" + returnUrl);
                }
                return;
            }
            string authorization = JwtManager.GetAuthorization(request.HttpContext, CookieKey);
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
                    string audience = GetRemoteIp(filterContext.HttpContext);
                    authorization = GetTokenByTicket(ticket, from, audience);
                    if (!string.IsNullOrEmpty(authorization))
                    {
                        SetCookies(filterContext.HttpContext.Response, authorization);
                    }
                    else
                    {
                        //ticket过期
                        if (absoluteUrl.Contains("ticket"))
                        {
                            var index = absoluteUrl.IndexOf("ticket");
                            absoluteUrl = absoluteUrl.Substring(0, index - 1);
                        }
                        filterContext.Result = GetActionResult(absoluteUrl);
                        return;
                    }
                }
            }
            try
            {
                var principal = JwtManager.ParseAuthorization(authorization, SecretKey);
                filterContext.HttpContext.User = principal;
                SetCookies(filterContext.HttpContext.Response, authorization);
                if (!CheckPermission(permissionName, authorization)) filterContext.Result = new ResponseModel<string>(ErrorCode.error_permission, "");
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
        private void SetCookies(HttpResponse httpResponse, string authorization)
        {
            if (CookieTime != "session")
            {
                httpResponse.Cookies.Append(CookieKey, authorization, new CookieOptions()
                {
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.Now.AddMinutes(Convert.ToInt32(CookieTime))
                });
            }
            else
            {
                httpResponse.Cookies.Append(CookieKey, authorization);
            }
        }
        private ActionResult GetActionResult(string returnUrl)
        {
            ActionResult result = new ResponseModel<string>(ErrorCode.authorize_fault, "");
            if (UnAuthorizedRedirect) result = new RedirectResult(BaseUrl.TrimEnd('/') + "/sso/login?returnUrl=" + returnUrl);
            return result;
        }
        private bool CheckPermission(string permission, string authorization)
        {
            if (permission.IsNullOrEmpty()) return true;
            var url = BaseUrl.TrimEnd('/') + "/permission/checkPermission?permissionName=" + permission;
            HttpRequestHelper httpRequestHelper = new HttpRequestHelper();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", authorization);
            string resp = httpRequestHelper.Get(url, headers);
            var result = JsonSerializerHelper.Deserialize<ServiceModel<string>>(resp);
            if (result.code == 0) return true;
            return false;
        }
        /// <summary>
        /// 获取远程ip
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetRemoteIp(HttpContext context)
        {
            return context.Connection.RemoteIpAddress.ToString();
        }
        /// <summary>
        /// 根据url上面的ticket获取token
        /// </summary>
        /// <param name="ticket"></param>
        /// <param name="from"></param>
        /// <param name="audience"></param>
        /// <returns></returns>
        public string GetTokenByTicket(string ticket, string from, string audience)
        {
            var url = BaseUrl.TrimEnd('/') + "/sso/gettoken?ticket=" + ticket + "&from=" + from + "&audience=" + audience;
            HttpRequestHelper httpRequestHelper = new HttpRequestHelper();
            string resp = httpRequestHelper.Get(url, null);
            var result = JsonSerializerHelper.Deserialize<ServiceModel<string>>(resp);
            if (result.code == 0) return result.result;
            return "";
        }
        /// <summary>
        /// 验证配置文件
        /// </summary>
        /// <param name="filterContext"></param>
        /// <returns></returns>
        public bool VerifyConfig(AuthorizationFilterContext filterContext)
        {
            if (BaseUrl.IsNullOrEmpty() && UnAuthorizedRedirect)
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
        /// <summary>
        /// 获取程序集所有带有 SSOAuthorizeAttribute 的名称列表
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        public static List<string> GetPermissionDescription(IEnumerable<Type> types)
        {
            List<string> actions = new List<string>();
            foreach (var item in types)
            {
                var methods = item.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(SSOAuthorizeAttribute));
                    foreach (Attribute att in attributes)
                    {
                        var name = ((SSOAuthorizeAttribute)att).Name;
                        if (!actions.Contains(name) && !string.IsNullOrEmpty(name)) actions.Add(name);
                    }
                }
            }
            return actions;
        }
    }
}
