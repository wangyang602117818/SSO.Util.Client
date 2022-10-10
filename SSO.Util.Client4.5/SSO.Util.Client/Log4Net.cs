using log4net;
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
        static string config = "<configuration>" +
            "<configSections>" +
                "<section name=\"log4net\" type=\"log4net.Config.Log4NetConfigurationSectionHandler,log4net\"/>" +
            "</configSections>" +
            "<log4net>" +
                "<logger name=\"FileLogAppender\">" +
                    "<level value=\"ALL\"/>" +
                "<appender-ref ref=\"FileLogAppender\"/>" +
                "</logger>" +
                "<appender name=\"FileLogAppender\" type=\"log4net.Appender.RollingFileAppender\">" +
                    "<staticLogFileName value=\"false\"/>" +
                    "<file value=\"App_Data/Log/\"/>" +
                    "<rollingStyle value=\"Composite\"/>" +
                    "<datePattern value=\"yyyy-MM-dd/yyyy-MM-dd HH&quot;.txt&quot;\"/>" +
                    "<maxSizeRollBackups value=\"30\"/>" +
                    "<maximumFileSize value=\"1000kb\"/>" +
                    "<layout type=\"log4net.Layout.PatternLayout\">" +
                        "<conversionPattern value=\"%d [%p]：%m%n\" />" +
                    "</layout>" +
                "</appender>" +
            "</log4net>" +
            "</configuration>";
        static Log4Net()
        {
            //string path = AppDomain.CurrentDomain.BaseDirectory + "bin\\SSO.Util.Client.dll";
            //if (!File.Exists(path))
            //{
            //    path = AppDomain.CurrentDomain.BaseDirectory + "SSO.Util.Client.dll";
            //}
            //var assembly = Assembly.LoadFrom(path);
            //var stream = assembly.GetManifestResourceStream("SSO.Util.Client.log4net.config");
            //log4net.Config.XmlConfigurator.Configure(stream);
            log4net.Config.XmlConfigurator.Configure(config.ToStream());
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
