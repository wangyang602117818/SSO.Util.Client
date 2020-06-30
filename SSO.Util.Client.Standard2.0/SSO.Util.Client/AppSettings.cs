using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace SSO.Util.Client
{
    /// <summary>
    /// 系统设置类
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// yyyy-MM-dd HH:mm:ss
        /// </summary>
        public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        /// <summary>
        /// 配置类,相当于.net的 ConfigurationManager
        /// </summary>
        public static IConfiguration Configuration;
        static AppSettings()
        {
            string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory());
            var jsonFile = $"appsettings.json";
            if (environment == "Development")
            {
                jsonFile = "appsettings.Development.json";
            }
            Configuration = builder.AddJsonFile(jsonFile, optional: false).Build();
        }
        /// <summary>
        /// 获取配置文件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValue(string key)
        {
            return Configuration.GetSection(key).Value;
        }
        /// <summary>
        /// 获取系统的当前url
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static string GetAbsoluteUri(HttpRequest httpRequest)
        {
            return httpRequest.Scheme + "://" + httpRequest.Host.ToString() + httpRequest.PathBase.Value + httpRequest.Path.Value + httpRequest.QueryString.ToString();
        }
    }
}
