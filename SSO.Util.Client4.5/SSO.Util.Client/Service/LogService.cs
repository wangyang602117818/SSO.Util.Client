using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 日志服务类
    /// </summary>
    public class LogService
    {
        private string baseUrl = "";
        HttpRequestHelper requestHelper = new HttpRequestHelper();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseUrl">日志项目的url</param>
        public LogService(string baseUrl)
        {
            this.baseUrl = baseUrl.TrimEnd('/');
        }
        /// <summary>
        /// 获取日志对象列表
        /// </summary>
        /// <param name="from"></param>
        /// <param name="controller"></param>
        /// <param name="action"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="sorts"></param>
        /// <param name="exception"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ServiceModel<List<LogModel>> GetList(string from = null, string to = null, string controller = null, string action = null, DateTime? startTime = null, DateTime? endTime = null, string userId = null, string userName = null, Dictionary<string, string> sorts = null, bool? exception = null, int pageIndex = 1, int pageSize = 10)
        {
            LogListModel logModel = new LogListModel()
            {
                From = from,
                To = to,
                ControllerName = controller,
                ActionName = action,
                StartTime = startTime,
                EndTime = endTime,
                UserId = userId,
                UserName = userName,
                Sorts = sorts,
                Exception = exception,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            var result = requestHelper.Post(baseUrl + "/log/getlist", logModel, null); ;
            return JsonSerializerHelper.Deserialize<ServiceModel<List<LogModel>>>(result);
        }
        /// <summary>
        /// 通过id获取日志详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ServiceModel<LogModel> GetById(string id)
        {
            var result = requestHelper.Get(baseUrl + "/log/getbyid/" + id, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<LogModel>>(result);
        }
        /// <summary>
        /// 获取最新的日志,按天统计
        /// </summary>
        /// <param name="last">最近多少天</param>
        /// <returns></returns>
        public ServiceModel<List<LogOpCountModel>> GetOpRecordByDay(int last = 30)
        {
            var result = requestHelper.Get(baseUrl + "/log/recordbyday?last=" + last, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<LogOpCountModel>>>(result);
        }
        /// <summary>
        /// 获取from列表
        /// </summary>
        /// <returns></returns>
        public ServiceModel<List<FromCountModel>> GetFromList()
        {
            var result = requestHelper.Get(baseUrl + "/log/getfromlist", null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<FromCountModel>>>(result);
        }
        /// <summary>
        /// 获取to列表
        /// </summary>
        /// <returns></returns>
        public ServiceModel<List<ToCountModel>> GetToList()
        {
            var result = requestHelper.Get(baseUrl + "/log/gettolist", null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<ToCountModel>>>(result);
        }
        /// <summary>
        /// 获取to中Controller列表
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public ServiceModel<List<ControllerCountModel>> GetControllersByTo(string to)
        {
            var result = requestHelper.Get(baseUrl + "/log/getcontrollersbyto?to=" + to, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<ControllerCountModel>>>(result);
        }
        /// <summary>
        /// 获取from中action列表
        /// </summary>
        /// <param name="to"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public ServiceModel<List<ActionCountModel>> GetActionsByController(string to, string controllerName)
        {
            var result = requestHelper.Get(baseUrl + "/log/getactionsbycontroller?to=" + to + "&controllerName=" + controllerName, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<ActionCountModel>>>(result);
        }
        /// <summary>
        /// 统计昨天,上月,全部日志
        /// </summary>
        /// <returns></returns>
        public ServiceModel<OperationsCountModel> GetOperations()
        {
            var result = requestHelper.Get(baseUrl + "/log/getoperations", null);
            return JsonSerializerHelper.Deserialize<ServiceModel<OperationsCountModel>>(result);
        }
    }
}
