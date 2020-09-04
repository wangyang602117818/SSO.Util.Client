using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 日志类
    /// </summary>
    public class LogModel
    {
        /// <summary>
        /// id
        /// </summary>
        public string _id { get; set; }
        /// <summary>
        /// 日志来源
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 日志访问的Controller
        /// </summary>
        public string Controller { get; set; }
        /// <summary>
        /// 日志访问的Action
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// 日志的路由信息
        /// </summary>
        public string Route { get; set; }
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
        public long Time { get; set; }
        /// <summary>
        /// 一分钟请求的次数
        /// </summary>
        public int CountPerMinute { get; set; }
        /// <summary>
        /// 是否有错误
        /// </summary>
        public bool Exception { get; set; }
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
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Dictionary<string, string> Sorts { get; set; }
        public bool? Exception { get; set; }
        public int PageIndex { get => pageIndex; set => pageIndex = value; }
        public int PageSize { get => pageSize; set => pageSize = value; }
    }
    public class FromCountModel
    {
        public string from { get; set; }
        public int count { get; set; }
    }
    public class ControllerCountModel
    {
        public string controller { get; set; }
        public int count { get; set; }
    }
    public class ActionCountModel
    {
        public string action { get; set; }
        public int count { get; set; }
    }
    public class OperationsCountModel
    {
        public long lastmonth { get; set; }
        public long lastday { get; set; }
        public long all { get; set; }
    }
}
