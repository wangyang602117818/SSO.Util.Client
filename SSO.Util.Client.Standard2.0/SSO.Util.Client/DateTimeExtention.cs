using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 时间扩展类
    /// </summary>
    public static class DateTimeExtention
    {
        /// <summary>
        /// 获取DateTime时间的时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string TimeStamp(this DateTime dateTime)
        {
            return ((dateTime.Ticks - 621355968000000000) / 10000000).ToString();
        }
        /// <summary>
        /// 获取DateTime时间的毫秒时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long MillisecondTimeStamp(this DateTime dateTime)
        {
            return (dateTime.Ticks - 621355968000000000) / 10000;
        }
        /// <summary>
        ///  时间戳转DateTime,如果是TUC时间则需要调用 .ToLocalTime() 转成当地时间
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime TimeStampToDateTime(this string timestamp)
        {
            DateTime time = DateTime.MinValue;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            if (timestamp.Length == 10)        //精确到秒
            {
                time = startTime.AddSeconds(double.Parse(timestamp));
            }
            else if (timestamp.Length == 13)   //精确到毫秒
            {
                time = startTime.AddMilliseconds(double.Parse(timestamp));
            }
            return time;
        }
        /// <summary>
        /// 毫秒时间戳转DateTime,如果是TUC时间则需要调用 .ToLocalTime() 转成当地时间
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime MilliTimeStampToDateTime(this string time)
        {
            return new DateTime((Convert.ToInt64(time) * 10000) + 621355968000000000);
        }
    }
}
