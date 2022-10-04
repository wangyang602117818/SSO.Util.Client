using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
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
        /// <param name="extra"></param>
        /// <param name="audience"></param>
        /// <returns></returns>
        public string GenerateToken(string userId, string userName, string lang, string audience, Dictionary<string, string> extra = null)
        {
            var symmetricKey = Convert.FromBase64String(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new List<Claim>() { new Claim(ClaimTypes.Name, userId) };
            if (!string.IsNullOrEmpty(userName)) claims.Add(new Claim("name", userName));
            if (!string.IsNullOrEmpty(lang)) claims.Add(new Claim("lang", lang));
            if (!string.IsNullOrEmpty(audience)) claims.Add(new Claim("from", audience));
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
                Expires = expires, //过期时间
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
        /// <param name="cookieKey"></param>
        /// <returns></returns>
        public static string GetAuthorization(string cookieKey = null)
        {
            if (cookieKey.IsNullOrEmpty()) cookieKey = SSOAuthorizeAttribute.CookieKey;
            string authorization = HttpContext.Current.Request.Cookies[cookieKey] == null ? "" : HttpContext.Current.Request.Cookies[cookieKey].Value;
            if (string.IsNullOrEmpty(authorization)) authorization = HttpContext.Current.Request.Headers["Authorization"] ?? "";
            if (string.IsNullOrEmpty(authorization)) authorization = HttpContext.Current.Request["Authorization"];
            return authorization;
        }
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="cookieKey"></param>
        /// <returns></returns>
        public static UserData GetUserData(string cookieKey = null)
        {
            string authorization = GetAuthorization(cookieKey);
            return ParseUserData(authorization);
        }
        /// <summary>
        /// 根据cookie和key解析用户信息
        /// </summary>
        /// <param name="authorization"></param>
        /// <param name="secretKey"></param>
        /// <param name="validateAudience">是否需要验证来源</param>
        /// <param name="validateExpiration">是否需要验证过期时间</param>
        /// <returns></returns>
        public static ClaimsPrincipal ParseAuthorization(string authorization, string secretKey = null, bool validateAudience = false, bool validateExpiration = true)
        {
            if (secretKey == null) secretKey = SSOAuthorizeAttribute.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var symmetricKey = Convert.FromBase64String(secretKey);
            string audience = "*";
            if (HttpContext.Current != null) audience = HttpContext.Current.Request.Url.Host.ReplaceHttpPrefix();
            var validationParameters = new TokenValidationParameters()
            {
                RequireExpirationTime = true,
                ValidateLifetime = validateExpiration,
                ValidateIssuer = false,
                ValidAudience = audience,
                ValidateAudience = validateAudience,
                IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
            };
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
            return new UserData()
            {
                From = User.Claims.Where(w => w.Type == "from").Select(s => s.Value).FirstOrDefault(),
                UserId = User.Identity.Name,
                UserName = User.Claims.Where(w => w.Type == "name").Select(s => s.Value).FirstOrDefault(),
                Lang = User.Claims.Where(w => w.Type == "lang").Select(s => s.Value).FirstOrDefault(),
            };
        }
        /// <summary>
        /// 根据cookie和key解析用户信息
        /// </summary>
        /// <param name="authorization"></param>
        /// <param name="secretKey"></param>
        /// <returns></returns>
        public static UserData ParseUserData(string authorization, string secretKey = null)
        {
            var principal = ParseAuthorization(authorization, secretKey, false, false);
            return ParseUserData(principal);
        }
    }
}
