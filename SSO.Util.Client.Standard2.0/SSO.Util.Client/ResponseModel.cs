using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// Action返回类
    /// </summary>
    /// <typeparam name="T">对象,不能为BsonDocument,如果为string,则string必须为json格式</typeparam>
    public class ResponseModel<T> : ContentResult
    {
        public ResponseModel(ErrorCode code, T t, long count = 0)
        {
            if (t is string)
            {
                string str = t.ToString();
                if ((str.Contains("{") && str.Contains("}")) || (str.Contains("[") && str.Contains("]")))
                {
                }
                else
                {
                    str = JsonSerializerHelper.Serialize(t);
                }
                Content = "{\"code\":" + (int)code + ",\"message\":\"" + code.ToString() + "\",\"result\":" + str + ",\"count\":" + count + "}";
            }
            else
            {
                Content = "{\"code\":" + (int)code + ",\"message\":\"" + code.ToString() + "\",\"result\":" + JsonSerializerHelper.Serialize(t) + ",\"count\":" + count + "}";
            }
            ContentType = "application/json";
        }
    }
    /// <summary>
    /// 解析Action返回类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceModel<T>
    {
        public int code { get; set; }
        public string message { get; set; }
        public T result { get; set; }
        public int count { get; set; }
    }
}
