﻿using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 默认log的位置  App_Data\Log\
    /// </summary>
    public static class Log4Net
    {
        private static readonly ILog infoLog = LogManager.GetLogger("FileLogAppender");
        static Log4Net()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "bin\\SSO.Util.Client.dll";
            if (!File.Exists(path))
            {
                path = AppDomain.CurrentDomain.BaseDirectory + "SSO.Util.Client.dll";
            }
            var assembly = Assembly.LoadFrom(path);
            var stream = assembly.GetManifestResourceStream("SSO.Util.Client.log4net.config");
            log4net.Config.XmlConfigurator.Configure(stream);
        }
        /// <summary>
        /// 错误日志 
        /// </summary>
        /// <param name="ex">异常信息</param>
        public static void ErrorLog(Exception ex)
        {
            infoLog.Error(ex);
        }
        /// <summary>
        /// 错误日志 
        /// </summary>
        /// <param name="str">异常信息</param>
        public static void ErrorLog(string str)
        {
            infoLog.Error(str);
        }
        /// <summary>
        /// 文本日志
        /// </summary>
        /// <param name="ex"></param>
        public static void InfoLog(Exception ex)
        {
            infoLog.Error(ex);
        }
        /// <summary>
        /// 文本日志
        /// </summary>
        /// <param name="str"></param>
        public static void InfoLog(string str)
        {
            infoLog.Info(str);
        }
    }
}
