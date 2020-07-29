using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// json序列化
    /// </summary>
    public class JsonSerializerHelper
    {
        static List<JsonConverter> converters = new List<JsonConverter>()
        {
            new IsoDateTimeConverter(){DateTimeFormat = AppSettings.DateTimeFormat}
        };
        static JsonSerializerSettings jSetting = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            Converters = converters
        };
        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, jSetting);
        }
        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string str)
        {
            return JsonConvert.DeserializeObject<T>(str, jSetting);
        }
    }
}
