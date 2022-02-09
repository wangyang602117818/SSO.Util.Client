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
    /// <summary>
    /// 上传文件返回类
    /// </summary>
    public class FileResponse
    {
        /// <summary>
        /// 文件id
        /// </summary>
        public string FileId { get; set; }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件字节数(大小)
        /// </summary>
        public long FileSize { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string Message { get; set; }
    }
}
