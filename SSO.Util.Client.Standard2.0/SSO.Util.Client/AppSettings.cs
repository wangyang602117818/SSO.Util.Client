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
            .SetBasePath(Path.Combine(AppContext.BaseDirectory));
            if (environment == "Development")
            {
                builder
                    .AddJsonFile(
                        Path.Combine(AppContext.BaseDirectory, string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar), $"appsettings.{environment}.json"),
                        optional: false
                    );
            }
            else
            {
                builder
                    .AddJsonFile($"appsettings.{environment}.json", optional: false);
            }
            Configuration = builder.Build();
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
            return httpRequest.Scheme + "://" + httpRequest.Host.ToString() + httpRequest.Path.ToString() + httpRequest.QueryString.ToString();
        }
    }
}
