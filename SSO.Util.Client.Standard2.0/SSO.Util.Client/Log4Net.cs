using log4net;
using log4net.Config;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    //默认log的位置  App_Data\Log\
    public static class Log4Net
    {
        static string repositoryName = "NETCoreRepository";
        static ILoggerRepository repository = LogManager.CreateRepository(repositoryName);
        private static readonly ILog infoLog = LogManager.GetLogger(repositoryName, "FileLogAppender");
        static Log4Net()
        {
            var assembly = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + "SSO.Util.Client.dll");
            var stream = assembly.GetManifestResourceStream("SSO.Util.Client.log4net.config");
            XmlConfigurator.Configure(repository, stream);
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
