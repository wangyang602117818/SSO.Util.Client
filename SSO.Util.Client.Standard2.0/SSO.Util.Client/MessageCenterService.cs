using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    public class MessageCenterService
    {
        private string baseUrl = "";
        HttpRequestHelper requestHelper = new HttpRequestHelper();
        public MessageCenterService(string baseUrl)
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
        public ServiceModel<string> InsertLog(string from, string controller, string action, string route, string querystring, string content, string userId, string userName, string userHost, string userAgent, long time = 0, bool exception = false)
        {
            LogModel logModel = new LogModel()
            {
                From = from,
                Controller = controller,
                Action = action,
                Route = route,
                QueryString = querystring,
                Content = content,
                UserId = userId,
                UserName = userName,
                UserHost = userHost,
                UserAgent = userAgent,
                Time = time,
                CountPerMinute = 1,
                Exception = exception,
                CreateTime = DateTime.Now
            };
            var result = requestHelper.Post(baseUrl.TrimEnd('/') + "/log/insert", logModel, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }

    }
}
