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
        /// <param name="ip"></param>
        /// <param name="minutes"></param>
        /// <returns></returns>
        public string GenerateToken(string userId, string userName, string lang, string ip, int minutes, Dictionary<string, string> extra = null)
        {
            var symmetricKey = Convert.FromBase64String(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new List<Claim>() { new Claim(ClaimTypes.Name, userId) };
            if (!string.IsNullOrEmpty(userName)) claims.Add(new Claim("StaffName", userName));
            if (!string.IsNullOrEmpty(lang)) claims.Add(new Claim("Lang", lang));
            if (extra != null)
            {
                foreach (var item in extra)
                {
                    claims.Add(new Claim(item.Key, item.Value));
                }
            }
            if (ip == "::1") ip = "127.0.0.1";
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),  //token数据
                Issuer = issuer,           //颁发者
                IssuedAt = DateTime.Now,               //颁发时间
                Audience = ip,                         //颁发给
                Expires = DateTime.Now.AddMinutes(minutes), //过期时间
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
        /// <param name="minutes"></param>
        /// <returns></returns>
        public string ModifyTokenLang(string token, string lang, int minutes)
        {
            var symmetricKey = Convert.FromBase64String(secretKey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var stoken = tokenHandler.ReadJwtToken(token);
            var newClaims = new List<Claim>() { };
            foreach (var claim in stoken.Claims)
            {
                if (claim.Type == "Lang")
                {
                    newClaims.Add(new Claim("Lang", lang));
                }
                else
                {
                    newClaims.Add(new Claim(claim.Type, claim.Value));
                }
            }
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(newClaims),  //token数据
                IssuedAt = DateTime.Now,               //颁发时间
                Expires = DateTime.Now.AddMinutes(minutes), //过期时间
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
            string ticket = SymmetricEncryptHelper.AesEncode(sourceString, secretKey);
            return Base64SecureURL.Encode(ticket);
        }
        /// <summary>
        /// 解析url上面的Ticket
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns>用户id,如果过期就返回""</returns>
        public string DecodeTicket(string ticket)
        {
            string sourceString = SymmetricEncryptHelper.AesDecode(Base64SecureURL.Decode(ticket), secretKey);
            string userId = sourceString.Substring(10, sourceString.Length - 18);
            DateTime ticketDateTime = DateTime.Parse(sourceString.Substring(0, 10) + " " + sourceString.Substring(10 + userId.Length));
            var diff = DateTime.Now - ticketDateTime;
            if (diff.TotalSeconds > ticketTime) return "";
            return userId;
        }

        /// <summary>
        /// 获取cookie或者请求header中的jwt token
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cookieKey"></param>
        /// <returns></returns>
        public static string GetAuthorization(HttpRequestBase request, string cookieKey = null)
        {
            if (cookieKey == null) cookieKey = SSOAuthorizeAttribute.CookieKey;
            string authorization = request.Cookies[cookieKey] == null ? "" : request.Cookies[cookieKey].Value;
            if (string.IsNullOrEmpty(authorization)) authorization = request.Headers["Authorization"] ?? "";
            if (string.IsNullOrEmpty(authorization)) authorization = request["Authorization"];
            return authorization;
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
                UserName = User.Claims.Where(w => w.Type == "StaffName").Select(s => s.Value).FirstOrDefault(),
                Lang = User.Claims.Where(w => w.Type == "Lang").Select(s => s.Value).FirstOrDefault(),
            };
        }
        /// <summary>
        /// 根据cookie和key解析用户信息
        /// </summary>
        /// <param name="authorization"></param>
        /// <param name="secretKey"></param>
        /// <param name="validateAudience">是否需要验证ip地址</param>
        /// <returns></returns>
        public static UserData ParseUserData(string authorization, string secretKey = null, bool validateAudience = false)
        {
            var principal = ParseAuthorization(authorization, secretKey, validateAudience);
            return ParseUserData(principal);
        }
        /// <summary>
        /// 根据cookie和key解析用户信息
        /// </summary>
        /// <param name="authorization"></param>
        /// <param name="secretKey"></param>
        /// <param name="validateAudience">是否需要验证ip地址</param>
        /// <returns></returns>
        public static ClaimsPrincipal ParseAuthorization(string authorization, string secretKey = null, bool validateAudience = false)
        {
            if (secretKey == null) secretKey = SSOAuthorizeAttribute.SecretKey;
            var tokenHandler = new JwtSecurityTokenHandler();
            var symmetricKey = Convert.FromBase64String(secretKey);
            string ip = HttpContext.Current.Request.UserHostAddress;
            if (ip == "::1") ip = "127.0.0.1";
            var validationParameters = new TokenValidationParameters()
            {
                RequireExpirationTime = true,
                ValidateLifetime = false,
                ValidateIssuer = false,
                ValidAudience = ip,
                ValidateAudience = validateAudience,
                IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
            };
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(authorization, validationParameters, out securityToken);
            return principal;
        }
    }
}
