using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;




namespace SSO.Util.Client.ElasticLite
{
    /// <summary>
    /// 操作 ElasticSearch 工具封装
    /// </summary>
    public class ElasticConnection
    {
        public static Queue<string> connections = new Queue<string>();
        public int count = 0;
        public string username;
        public string password;
        public int Timeout { get; set; }
        /// <summary>
        /// 单个 ElasticSearch 服务器实例化
        /// </summary>
        /// <param name="url">ElasticSearch服务器地址</param>
        /// <param name="username">ElasticSearch用户名</param>
        /// <param name="password">ElasticSearch密码</param>
        /// <param name="timeout">超时时间s</param>
        public ElasticConnection(string url, string username = "", string password = "", int timeout = 6)
        {
            if (connections == null)
                connections = new Queue<string>(new List<string>() { url });
            count = 1;
            this.username = username;
            this.password = password;
            Timeout = timeout;
        }
        /// <summary>
        /// 集群 ElasticSearch 服务器实例化
        /// </summary>
        /// <param name="urls">ElasticSearch服务器地址列表</param>
        /// <param name="username">ElasticSearch用户名</param>
        /// <param name="password">ElasticSearch密码</param>
        /// <param name="timeout">超时时间</param>
        public ElasticConnection(IEnumerable<string> urls, string username = "", string password = "", int timeout = 6)
        {
            if (connections == null)
                connections = new Queue<string>(urls);
            count = connections.Count;
            this.username = username;
            this.password = password;
            Timeout = timeout;
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="command">person/doc/1</param>
        /// <param name="jsonData">删除条件</param>
        /// <returns></returns>
        public async Task<string> Delete(string command, string jsonData = null)
        {
            return await ExecuteRequest(HttpMethod.Delete, command, jsonData);
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="command">person/doc/1</param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public async Task<string> Get(string command, string jsonData = null)
        {
            return await ExecuteRequest(HttpMethod.Get, command, jsonData);
        }
        /// <summary>
        /// 判断index是否存在
        /// </summary>
        /// <param name="command">indexName</param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public async Task<bool> Head(string command, string jsonData = null)
        {
            var result = ExecuteRequest(HttpMethod.Head, command, jsonData);
            return await result != "404" ? true : false;
        }
        /// <summary>
        /// 索引数据
        /// </summary>
        /// <param name="command">person/doc/1</param>
        /// <param name="jsonData">数据对象</param>
        /// <returns></returns>
        public async Task<string> Post(string command, string jsonData = null)
        {
            return await ExecuteRequest(HttpMethod.Post, command, jsonData);
        }
        /// <summary>
        /// 创建 index, 并且设置mapping:
        /// </summary>
        /// <param name="command">indexName</param>
        /// <param name="jsonData">mapping</param>
        /// <returns></returns>
        public async Task<string> Put(string command, string jsonData = null)
        {
            return await ExecuteRequest(HttpMethod.Put, command, jsonData);
        }
        private async Task<string> ExecuteRequest(HttpMethod method, string command, string jsonData)
        {
            HttpRequestException ex = null;
            for (var i = 0; i < count; i++)
            {
                //从队列获取一个连接
                string uri = connections.Peek();
                uri = uri.TrimEnd('/') + "/" + command.TrimStart('/');
                try
                {
                    var handler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
                    };
                    var request = new HttpRequestMessage
                    {
                        Method = method,
                        RequestUri = new Uri(uri)
                    };
                    if (!string.IsNullOrEmpty(jsonData)) request.Content = new StringContent(jsonData ?? "{}", Encoding.UTF8, "application/json");
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    using (var httpClient = new HttpClient(handler))
                    {
                        httpClient.Timeout = TimeSpan.FromSeconds(Timeout);
                        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                        {
                            string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);
                        }
                        var response = await httpClient.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        return await response.Content.ReadAsStringAsync();
                    }
                }
                catch (HttpRequestException exception)
                {
                    //服务可用并且返回404
                    if (exception.StatusCode == HttpStatusCode.NotFound) return "404";
                    ex = exception;
                    //从队列获取的连接不可用
                    string unuseConnect = connections.Dequeue();
                    //把不可用的连接放入队尾
                    connections.Enqueue(unuseConnect);
                }
            }
            if (ex != null)
            {
                throw ex;
            }
            return ex.Message;
        }
    }
}
