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
            this.baseUrl = baseUrl.TrimEnd('/');
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
            var result = requestHelper.Post(baseUrl + "/filetask/insert", new { machineName, collectionName, collectionId }, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        /// <summary>
        /// 添加调度任务
        /// </summary>
        /// <param name="machineName"></param>
        /// <param name="schedulingId"></param>
        /// <param name="triggerId"></param>
        /// <param name="schedulingState"></param>
        /// <returns></returns>
        public ServiceModel<string> InsertTaskScheduling(string machineName, int schedulingId, int triggerId, int schedulingState)
        {
            var result = requestHelper.Post(baseUrl + "/TaskScheduling/insert", new { machineName, schedulingId, triggerId, schedulingState }, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
    }
}
