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
        public static long TimeStamp(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
        }
        /// <summary>
        /// 获取DateTime时间的毫秒时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long MillisecondTimeStamp(this DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }
        /// <summary>
        ///  时间戳返回当地时间(.ToUniversalTime() 转成UTC时间)
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public static DateTime TimeStampToDateTime(this long timestamp)
        {
            DateTime time = DateTime.MinValue;
            DateTime startTime = TimeZoneInfo.ConvertTimeFromUtc(new DateTime(1970, 1, 1, 0, 0, 0, 0), TimeZoneInfo.Local);
            var len = timestamp.ToString().Length;
            if (len == 10)        //精确到秒
            {
                time = startTime.AddSeconds(timestamp);
            }
            else if (len == 13)   //精确到毫秒
            {
                time = startTime.AddMilliseconds(timestamp);
            }
            return time;
        }
    }
}
