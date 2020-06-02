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
        public ServiceModel<string> Insert(string from, string controller, string action, string querystring, string content, string userId, string userName, string userHost, string userAgent, int time = 0)
        {
            LogModel logModel = new LogModel()
            {
                From = from,
                Controller = controller,
                Action = action,
                QueryString = querystring,
                Content = content,
                UserId = userId,
                UserName = userName,
                UserHost = userHost,
                UserAgent = userAgent,
                Time = time,
                CreateTime = DateTime.Now
            };
            var result = requestHelper.Post(baseUrl.TrimEnd('/') + "/log/insert", logModel, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        public string GetListJson(string from = null, string userId = null, Dictionary<string, string> sorts = null, int pageIndex = 1, int pageSize = 10)
        {
            LogListModel logModel = new LogListModel()
            {
                From = from,
                UserId = userId,
                Sorts = sorts,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return requestHelper.Post(baseUrl.TrimEnd('/') + "/log/getlist?", logModel, null);
        }
        public ServiceModel<List<LogModel>> GetList(string from = null, string userId = null, Dictionary<string, string> sorts = null, int pageIndex = 1, int pageSize = 10)
        {
            var result = GetListJson(from, userId, sorts, pageIndex, pageSize);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<LogModel>>>(result);
        }
        public string GetOpRecordByDayJson(int last = 30)
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/recordbyday?last=" + last, null);
        }
        public ServiceModel<List<LogOpCountModel>> GetOpRecordByDay(int last = 30)
        {
            var result = GetOpRecordByDayJson(last);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<LogOpCountModel>>>(result);
        }
    }
}
