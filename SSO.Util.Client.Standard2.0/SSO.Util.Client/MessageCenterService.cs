using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 队列信息中心服务
    /// </summary>
    public class MessageCenterService
    {
        private string baseUrl = "";
        HttpRequestHelper requestHelper = new HttpRequestHelper();
        /// <summary>
        /// 消息中心
        /// </summary>
        /// <param name="baseUrl">消息中心项目的基本url</param>
        public MessageCenterService(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <param name="route"></param>
        /// <param name="querystring"></param>
        /// <param name="requestContent"></param>
        /// <param name="responseContent"></param>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="userHost"></param>
        /// <param name="userAgent"></param>
        /// <param name="time"></param>
        /// <param name="exception"></param>
        /// <returns></returns>
        public ServiceModel<string> InsertLog(string from, string to, string controller, string action, string route, string querystring, string requestContent, string responseContent, string userId, string userName, string userHost, string userAgent, long time = 0, bool exception = false)
        {
            LogModel logModel = new LogModel()
            {
                From = from,
                To = to,
                Controller = controller,
                Action = action,
                Route = route,
                QueryString = querystring,
                Content = requestContent,
                Response = responseContent,
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
        /// <summary>
        /// 添加转换任务
        /// </summary>
        /// <param name="machineName">发往那个机器</param>
        /// <param name="collectionName"></param>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        public ServiceModel<string> InsertConvertTask(string machineName, string collectionName, string collectionId)
        {
            var result = requestHelper.Post(baseUrl.TrimEnd('/') + "/filetask/insert", new { machineName, collectionName, collectionId }, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
    }
}
