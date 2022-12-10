using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client.ElasticLite
{
    /// <summary>
    /// 操作 ElasticSearch 工具封装
    /// </summary>
    public class ElasticConnection
    {
        public static Queue<string> connections = null;
        public int count = 0;
        public int Timeout { get; set; }
        /// <summary>
        /// 单个 ElasticSearch 服务器实例化
        /// </summary>
        /// <param name="url">ElasticSearch服务器地址</param>
        /// <param name="timeout">超时时间</param>
        public ElasticConnection(string url, int timeout = 6000)
        {
            if (connections == null)
                connections = new Queue<string>(new List<string>() { url });
            count = 1;
            Timeout = timeout;
        }
        /// <summary>
        /// 集群 ElasticSearch 服务器实例化
        /// </summary>
        /// <param name="urls">ElasticSearch服务器地址列表</param>
        /// <param name="timeout">超时时间</param>
        public ElasticConnection(IEnumerable<string> urls, int timeout = 6000)
        {
            if (connections == null)
                connections = new Queue<string>(urls);
            count = connections.Count;
            Timeout = timeout;
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="command">person/doc/1</param>
        /// <param name="jsonData">删除条件</param>
        /// <returns></returns>
        public string Delete(string command, string jsonData = null)
        {
            return ExecuteRequest("DELETE", command, jsonData);
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="command">person/doc/1</param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public string Get(string command, string jsonData = null)
        {
            return ExecuteRequest("GET", command, jsonData);
        }
        /// <summary>
        /// 判断index是否存在
        /// </summary>
        /// <param name="command">indexName</param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public bool Head(string command, string jsonData = null)
        {
            var result = ExecuteRequest("HEAD", command, jsonData);
            return result != "404" ? true : false;
        }
        /// <summary>
        /// 索引数据
        /// </summary>
        /// <param name="command">person/doc/1</param>
        /// <param name="jsonData">数据对象</param>
        /// <returns></returns>
        public string Post(string command, string jsonData = null)
        {
            return ExecuteRequest("Post", command, jsonData);
        }
        /// <summary>
        /// 创建 index, 并且设置mapping:
        /// </summary>
        /// <param name="command">indexName</param>
        /// <param name="jsonData">mapping</param>
        /// <returns></returns>
        public string Put(string command, string jsonData = null)
        {
            return ExecuteRequest("Put", command, jsonData);
        }
        private string ExecuteRequest(string method, string command, string jsonData)
        {
            WebException ex = null;
            for (var i = 0; i < count; i++)
            {
                //从队列获取一个连接
                string uri = connections.Peek();
                uri = uri.TrimEnd('/') + "/" + command.TrimStart('/');
                try
                {
                    HttpWebRequest request = CreateRequest(method, uri);
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(jsonData);
                        request.ContentLength = buffer.Length;
                        using (Stream requestStream = request.GetRequestStream())
                        {
                            requestStream.Write(buffer, 0, buffer.Length);
                        }
                    }
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                        {
                            ex = null;
                            return reader.ReadToEnd();
                        }
                    }
                }
                catch (WebException webException)
                {
                    //服务可用并且返回404
                    if (webException.Response != null && ((HttpWebResponse)webException.Response).StatusCode == HttpStatusCode.NotFound)
                    {
                        using (StreamReader reader = new StreamReader(webException.Response.GetResponseStream()))
                        {
                            ex = null;
                            string resp = reader.ReadToEnd();
                            if (resp.IsNullOrEmpty()) return "404";
                            return resp;
                        }
                    }
                    ex = webException;
                    //从队列获取的连接不可用
                    string unuseConnect = connections.Dequeue();
                    //把不可用的连接放入队尾
                    connections.Enqueue(unuseConnect);
                    //通知维护人员
                    Log4Net.ErrorLog(webException);
                }
            }
            if (ex != null)
            {
                throw ex;
            }
            return ex.Message;
        }
        private HttpWebRequest CreateRequest(string method, string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            request.ServerCertificateValidationCallback = ((message, cert, chain, error) => { return true; });
            request.Accept = "application/json";
            request.ContentType = "application/json";
            request.Timeout = Timeout;
            return request;
        }
    }
}
