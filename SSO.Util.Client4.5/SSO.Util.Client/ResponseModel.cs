using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SSO.Util.Client
{
    /// <summary>
    /// Action返回类
    /// </summary>
    /// <typeparam name="T">对象,不能为BsonDocument</typeparam>
    public class ResponseModel<T> : ContentResult
    {
        public ResponseModel(ErrorCode code, T t, long count = 0)
        {
            if (t is string)
            {
                Content = "{\"code\":" + (int)code + ",\"message\":\"" + code.ToString() + "\",\"result\":\"" + t.ToString().Replace("\"", "\\\"") + "\",\"count\":" + count + "}";
            }
            else
            {
                Content = "{\"code\":" + (int)code + ",\"message\":\"" + code.ToString() + "\",\"result\":" + JsonSerializerHelper.Serialize(t) + ",\"count\":" + count + "}";
            }
            ContentEncoding = Encoding.UTF8;
            ContentType = "application/json";
        }
    }
}
