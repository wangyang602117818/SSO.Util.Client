﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SSO.Util.Client
{
    /// <summary>
    /// Jwt管理类,使用之前需要在web.config配置 secretKey issuer(颁发者) ticketTime(url上面的票据有效时间,单位秒)
    /// </summary>
    public class JwtManager
    {
        private string secretKey = "";
        private string issuer = "";
        private int ticketTime = 0;
        private DateTime expires = DateTime.Now.Date.AddDays(1).AddHours(2);
        /// <summary>
        /// 生成JwtToken的类
        /// </summary>
        /// <param name="secretKey">秘钥</param>
        /// <param name="issuer">颁发者</param>
        /// <param name="ticketTime">url上面的ticket过期时间(秒)</param>
        public JwtManager(string secretKey, string issuer, int ticketTime = 0)
        {
            this.secretKey = secretKey;
            this.issuer = issuer;
            this.ticketTime = ticketTime;
        }
        /// <summary>
        /// 生成JwtToken类
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="lang"></param>
        /// <param name="audience">颁发给(客户端地址|ip)</param>
        /// <param name="from">token来源(项目地址)</param>
        ///  <param name="extra">其他信息</param>
        /// <returns></returns>
        public string GenerateToken(string userId, string userName, string lang, string audience, string from, Dictionary<string, string> extra = null)
        {
            var symmetricKey = Convert.FromBase64String(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new List<Claim>() { new Claim(ClaimTypes.Name, userId) };
            if (!string.IsNullOrEmpty(userName)) claims.Add(new Claim("name", userName));
            if (!string.IsNullOrEmpty(lang)) claims.Add(new Claim("lang", lang));
            if (!string.IsNullOrEmpty(from)) claims.Add(new Claim("from", from));
            if (extra != null)
            {
                foreach (var item in extra)
                {
                    claims.Add(new Claim(item.Key, item.Value));
                }
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),  //token数据
                Issuer = issuer,           //颁发者
                IssuedAt = DateTime.Now,               //颁发时间
                Audience = audience,                         //颁发给
                Expires = expires,         //过期时间
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)   //签名
            };
            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);
            return token;
        }
        /// <summary>
        /// 改变语言,并且返回新token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public string ModifyTokenLang(string token, string lang)
        {
            var symmetricKey = Convert.FromBase64String(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var stoken = tokenHandler.ReadJwtToken(token);
            var newClaims = new List<Claim>() { };
            DateTime expTime = DateTime.Now;
            foreach (var claim in stoken.Claims)
            {
                if (claim.Type == "lang")
                {
                    newClaims.Add(new Claim("lang", lang));
                }
                else
                {
                    newClaims.Add(new Claim(claim.Type, claim.Value));
                }
                if (claim.Type == "exp")
                {
                    expTime = Convert.ToInt64(claim.Value).TimeStampToDateTime();
                }
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(newClaims),  //token数据
                IssuedAt = DateTime.Now,               //颁发时间
                Expires = expTime, //过期时间
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)   //签名
            };
            var newStoken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(newStoken);
        }
        /// <summary>
        /// 生成url上面的Ticket,一般只有几秒有效期
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public string GenerateTicket(string userId)
        {
            string sourceString = DateTime.Now.ToString("yyyy-MM-dd") + userId + DateTime.Now.ToString("HH:mm:ss");
            string ticket = AesEncryptHelper.Encode(sourceString, secretKey);
            return Base64SecureURL.Encode(ticket);
        }
        /// <summary>
        /// 解析url上面的Ticket
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns>用户id,如果过期就返回""</returns>
        public string DecodeTicket(string ticket)
        {
            string sourceString = AesEncryptHelper.Decode(Base64SecureURL.Decode(ticket), secretKey);
            string userId = sourceString.Substring(10, sourceString.Length - 18);
            DateTime ticketDateTime = DateTime.Parse(sourceString.Substring(0, 10) + " " + sourceString.Substring(10 + userId.Length));
            var diff = DateTime.Now - ticketDateTime;
            if (diff.TotalSeconds > ticketTime) return "";
            return userId;
        }
        /// <summary>
        /// 获取cookie或者请求header中的jwt token
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="cookieKey"></param>
        /// <returns></returns>
        public static string GetAuthorization(HttpContext httpContext, string cookieKey = null)
        {
            if (cookieKey.IsNullOrEmpty()) cookieKey = SSOAuthorizeAttribute.CookieKey;
            string authorization = httpContext.Request.Cookies[cookieKey] == null ? "" : httpContext.Request.Cookies[cookieKey];
            if (string.IsNullOrEmpty(authorization)) authorization = httpContext.Request.Headers["Authorization"].ToString() ?? "";
            if (string.IsNullOrEmpty(authorization)) authorization = httpContext.Request.Query["Authorization"];
            return authorization;
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="cookieKey"></param>
        /// <returns></returns>
        public static UserData GetUserData(HttpContext httpContext, string cookieKey = null)
        {
            string authorization = GetAuthorization(httpContext, cookieKey);
            if (string.IsNullOrEmpty(authorization)) return null;
            return ParseUserData(authorization);
        }
        /// <summary>
        /// 根据cookie和key解析用户信息
        /// </summary>
        /// <param name="authorization"></param>
        /// <param name="secretKey"></param>
        /// <param name="validateAudience">是否需要验证来源</param>
        /// <param name="audience">来源</param>
        /// <param name="validateExpiration">是否需要验证过期时间</param>
        /// <returns></returns>
        public static ClaimsPrincipal ParseAuthorization(string authorization, string secretKey = null, bool validateAudience = false, string audience = null, bool validateExpiration = false)
        {
            if (secretKey == null) secretKey = SSOAuthorizeAttribute.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var symmetricKey = Convert.FromBase64String(secretKey);
            var validationParameters = new TokenValidationParameters()
            {
                RequireExpirationTime = true,
                ValidateLifetime = validateExpiration,
                ValidateIssuer = false,
                ValidateAudience = validateAudience,
                IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
            };
            if (!audience.IsNullOrEmpty()) validationParameters.ValidAudience = audience;
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(authorization, validationParameters, out securityToken);
            return principal;
        }
        /// <summary>
        /// 解析出用户信息
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static UserData ParseUserData(ClaimsPrincipal User)
        {
            UserData userData = new UserData() { Extra = new Dictionary<string, string>() };
            foreach (var item in User.Claims)
            {
                var list = new List<string>() { "aud", "nbf", "iss", "exp", "iat" };
                if (list.Contains(item.Type)) continue;
                if (item.Type == ClaimTypes.Name)
                {
                    userData.UserId = item.Value;
                }
                else if (item.Type == "name")
                {
                    userData.UserName = item.Value;
                }
                else if (item.Type == "lang")
                {
                    userData.Lang = item.Value;
                }
                else if (item.Type == "from")
                {
                    userData.From = item.Value;
                }
                else
                {
                    userData.Extra.Add(item.Type, item.Value);
                }
            }
            return userData;
        }
        /// <summary>
        /// 根据cookie和key解析用户信息
        /// </summary>
        /// <param name="authorization"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static UserData ParseUserData(string authorization, string secretKey = null)
        {
            var principal = ParseAuthorization(authorization, secretKey, false, null, false);
            return ParseUserData(principal);
        }
    }
}
