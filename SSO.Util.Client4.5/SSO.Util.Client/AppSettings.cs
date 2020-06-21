using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    public static class AppSettings
    {
        /// <summary>
        /// yyyy-MM-dd HH:mm:ss
        /// </summary>
        public static string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public static string GetValue(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }
    }
}
