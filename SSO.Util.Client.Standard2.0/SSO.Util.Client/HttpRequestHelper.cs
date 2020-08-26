using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// http请求类
    /// </summary>
    public class HttpRequestHelper
    {
        /// <summary>
        /// 发送文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="paramName"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType"></param>
        /// <param name="fileStream"></param>
        /// <param name="paras"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public string PostFile(string url, string paramName, string fileName, string contentType, Stream fileStream, Dictionary<string, string> paras = null, Dictionary<string, string> headers = null)
        {
            string boundary = "----" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 30);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "post";
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }
            //请求流
            using (Stream requestStream = request.GetRequestStream())
            {
                //文件开始标记
                string fileBegin = "--" + boundary + "\r\nContent-Disposition: form-data;name=\"" + paramName + "\";filename=\"" + fileName.GetFileName() + "\"\r\nContent-Type: "+ contentType + "; charset=utf-8\r\n\r\n";
                byte[] bytes = Encoding.UTF8.GetBytes(fileBegin);
                requestStream.Write(bytes, 0, bytes.Length);
                ////传文件数据
                fileStream.Position = 0;
                fileStream.CopyTo(requestStream);
                //传换行数据
                byte[] LFBytes = Encoding.UTF8.GetBytes("\r\n");
                requestStream.Write(LFBytes, 0, LFBytes.Length);
                StringBuilder sb_params = new StringBuilder();
                if (paras != null)
                {
                    foreach (string key in paras.Keys)
                    {
                        sb_params.Append("--" + boundary + "\r\n");
                        sb_params.Append("Content-Disposition: form-data; name=\"" + key + "\"\r\n\r\n");
                        sb_params.Append(paras[key] + "\r\n");
                    }
                }
                byte[] paramsBytes = Encoding.UTF8.GetBytes(sb_params.ToString());
                requestStream.Write(paramsBytes, 0, paramsBytes.Length);
                //结束标记
                byte[] byte1 = Encoding.UTF8.GetBytes("--" + boundary + "--");  //文件结束标志prefix很重要
                requestStream.Write(byte1, 0, byte1.Length);
            }
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="obj"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public string Post(string url, object obj, Dictionary<string, string> headers)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "post";
            request.ContentType = "application/json";
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                string json = JsonConvert.SerializeObject(obj);
                streamWriter.Write(json);
            }
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        /// <summary>
        /// get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public string Get(string url, Dictionary<string, string> headers)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "get";
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public DownloadFileItem GetFile(string url, Dictionary<string, string> headers)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "get";
            if (headers != null)
            {
                foreach (var kv in headers)
                {
                    request.Headers.Add(kv.Key, kv.Value);
                }
            }
            WebResponse response = request.GetResponse();
            string name = "";
            if (response.Headers["Content-Disposition"] != null)
            {
                string[] names = response.Headers["Content-Disposition"].Split('=');
                name = names[1].Trim('"');
            }
            return new DownloadFileItem()
            {
                FileName = name,
                ContentType = response.ContentType,
                ContentLength = response.ContentLength,
                FileStream = response.GetResponseStream()
            };
        }
    }
}
