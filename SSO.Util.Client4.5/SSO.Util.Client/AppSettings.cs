using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SSO.Util.Client
{
    /// <summary>
    /// 系统设置类
    /// </summary>
    public static class AppSettings
    {
        /// <summary>
        /// yyyy-MM-dd HH:mm:ss
        /// </summary>
        public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        /// <summary>
        /// 获取配置文件
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
        /// <summary>
        /// 获取应用程序的根路径
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetApplicationUrl(HttpRequestBase request)
        {
            return request.Url.Scheme + "://" + request.Url.Host + ":" + request.Url.Port + request.ApplicationPath;
        }
        /// <summary>
        /// 获取当前请求的完整路径
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetAbsoluteUri(HttpRequestBase request)
        {
            return request.Url.Scheme + "://" + request.Url.Host + ":" + request.Url.Port + request.ApplicationPath;
        }
    }
}
