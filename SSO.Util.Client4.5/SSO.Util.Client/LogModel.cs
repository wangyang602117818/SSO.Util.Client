using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    public class LogModel
    {
        /// <summary>
        /// 日志来源
        /// </summary>
        public string From { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        /// <summary>
        /// 请求参数
        /// </summary>
        public string QueryString { get; set; }
        /// <summary>
        /// 请求内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 日志关联人
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 日志关联人
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户ip
        /// </summary>
        public string UserHost { get; set; }
        /// <summary>
        /// 用户代理
        /// </summary>
        public string UserAgent { get; set; }
        /// <summary>
        /// 响应时间(毫秒)
        /// </summary>
        public int Time { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }

    public class LogOpCountModel
    {
        public string date { get; set; }
        public int count { get; set; }
    }
    public class LogListModel
    {
        private int pageIndex = 1;
        private int pageSize = 10;
        public string From { get; set; }
        public string UserId { get; set; }
        public Dictionary<string, string> Sorts { get; set; }
        public int PageIndex { get => pageIndex; set => pageIndex = value; }
        public int PageSize { get => pageSize; set => pageSize = value; }
    }
}
