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
            var port = request.Url.Port;
            var portstr = port == 80 ? "" : (":" + port);
            return request.Url.Scheme + "://" + request.Url.Host + portstr + request.ApplicationPath;
        }
        /// <summary>
        /// 获取应用程序的根路径
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetApplicationUrl(HttpRequest request)
        {
            var port = request.Url.Port;
            var portstr = port == 80 ? "" : (":" + port);
            return request.Url.Scheme + "://" + request.Url.Host + portstr + request.ApplicationPath;
        }
        /// <summary>
        /// 获取应用程序的根路径,并且替换前缀http和后缀/,然后转成小写
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetApplicationUrlTrimHttpPrefix(HttpRequestBase request)
        {
            return GetApplicationUrl(request).ReplaceHttpPrefix().TrimEnd('/').ToLower();
        }
        /// <summary>
        /// 获取应用程序的根路径,并且替换前缀http和后缀/,然后转成小写
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetApplicationUrlTrimHttpPrefix(HttpRequest request)
        {
            return GetApplicationUrl(request).ReplaceHttpPrefix().TrimEnd('/').ToLower();
        }
        /// <summary>
        /// 获取当前请求的完整路径
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetAbsoluteUri(HttpRequestBase request)
        {
            return request.Url.AbsoluteUri;
        }
        /// <summary>
        /// 获取当前请求的完整路径
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetAbsoluteUri(HttpRequest request)
        {
            return request.Url.AbsoluteUri;
        }
    }
}
