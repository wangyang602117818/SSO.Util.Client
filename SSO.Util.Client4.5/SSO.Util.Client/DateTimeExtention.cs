using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UtilToolkit
{
    public static class DateTimeExtention
    {
        public static string UTCTimeStamp(this DateTime dateTime)
        {
            return ((dateTime.Ticks - 621355968000000000) / 10000000).ToString();
        }

        public static string BeijingTimeStamp(this DateTime dateTime)
        {
            return ((dateTime.Ticks - 621355968000000000) / 10000000).ToString();
        }
    }
}
