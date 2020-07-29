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
    /// <typeparam name="T">对象,不能为BsonDocument,如果为string,则string必须为json格式</typeparam>
    public class ResponseModel<T> : ContentResult
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="t"></param>
        /// <param name="count"></param>
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
            ContentEncoding = Encoding.UTF8;
            ContentType = "application/json";
        }
    }
    /// <summary>
    /// 解析Action返回类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServiceModel<T>
    {
        /// <summary>
        /// 返回码
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// 返回信息
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 返回内容
        /// </summary>
        public T result { get; set; }
        /// <summary>
        /// 返回数据的总数
        /// </summary>
        public int count { get; set; }
    }
}
