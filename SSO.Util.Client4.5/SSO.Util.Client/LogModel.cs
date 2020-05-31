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
        /// <summary>
        /// 日志类型
        /// </summary>
        public LogType Type { get; set; }
        /// <summary>
        /// 日志唯一id
        /// </summary>
        public string RecordId { get; set; }
        /// <summary>
        /// 日志内容
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
    public enum LogType
    {
        /// <summary>
        /// 列表日志
        /// </summary>
        List = 0,
        /// <summary>
        /// 添加
        /// </summary>
        Add = 1,
        /// <summary>
        /// 详情日志
        /// </summary>
        Query = 2,
        /// <summary>
        /// 更新日志
        /// </summary>
        Update = 3,
        /// <summary>
        /// 删除日志
        /// </summary>
        Delete = 4,
        /// <summary>
        /// 错误日志
        /// </summary>
        Error = 1000,
    }
}
