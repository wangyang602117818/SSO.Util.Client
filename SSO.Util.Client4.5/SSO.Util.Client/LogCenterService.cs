using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    public class LogCenterService
    {
        private string baseUrl = "";
        HttpRequestHelper requestHelper = new HttpRequestHelper();
        public LogCenterService(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="from">日志来源</param>
        /// <param name="type">日志类型</param>
        /// <param name="id">日志唯一id</param>
        /// <param name="content">日志内容</param>
        /// <param name="userId">用户id</param>
        /// <param name="userName">用户名称</param>
        /// <param name="userHost">用户主机</param>
        /// <param name="userAgent">用户代理</param>
        /// <param name="time">时长</param>
        /// <returns></returns>
        public ServiceModel<string> Insert(string from, LogType type, string id, string content, string userId, string userName, string userHost, string userAgent, int time = 0)
        {
            LogModel logModel = new LogModel()
            {
                From = from,
                Type = type,
                RecordId = id,
                Content = content,
                UserId = userId,
                UserName = userName,
                UserHost = userHost,
                UserAgent = userAgent,
                Time = time
            };
            var result = requestHelper.Post(baseUrl.TrimEnd('/') + "/log/insert", logModel, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        public ServiceModel<List<LogModel>> GetList(string from = null, LogType? type = null, string userId = null, Dictionary<string, string> sorts = null, int pageIndex = 1, int pageSize = 10)
        {
            List<string> filter = new List<string>();
            filter.Add("pageIndex=" + pageIndex);
            filter.Add("pageSize=" + pageSize);
            if (!from.IsNullOrEmpty()) filter.Add("from=" + from);
            if (type != null) filter.Add("type=" + type);
            if (userId != null) filter.Add("userId=" + userId);
            if (sorts != null) filter.Add("sorts=" + JsonSerializerHelper.Serialize(sorts));
            string query = string.Join("&", filter);
            var result = requestHelper.Get(baseUrl.TrimEnd('/') + "/log/getlist?" + query, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<LogModel>>>(result);
        }
    }
}
