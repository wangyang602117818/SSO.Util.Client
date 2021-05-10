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
            return ((dateTime.Ticks - 621355968000000000) / 10000000);
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
