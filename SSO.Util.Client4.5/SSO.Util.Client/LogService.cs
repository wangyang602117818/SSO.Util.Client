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
            this.baseUrl = baseUrl;
        }
        /// <summary>
        /// 获取日志列表的json字符串
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
        public string GetListJson(string from = null, string controller = null, string action = null, DateTime? startTime = null, DateTime? endTime = null, string userId = null, string userName = null, Dictionary<string, string> sorts = null, bool? exception = null, int pageIndex = 1, int pageSize = 10)
        {
            LogListModel logModel = new LogListModel()
            {
                From = from,
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
            return requestHelper.Post(baseUrl.TrimEnd('/') + "/log/getlist?", logModel, null);
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
        public ServiceModel<List<LogModel>> GetList(string from = null, string controller = null, string action = null, DateTime? startTime = null, DateTime? endTime = null, string userId = null, string userName = null, Dictionary<string, string> sorts = null, bool? exception = null, int pageIndex = 1, int pageSize = 10)
        {
            var result = GetListJson(from, controller, action, startTime, endTime, userId, userName, sorts, exception, pageIndex, pageSize);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<LogModel>>>(result);
        }
        /// <summary>
        /// 获取日志列表的json字符串简单模式
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
        public string GetListSimpleJson(string from = null, string controller = null, string action = null, DateTime? startTime = null, DateTime? endTime = null, string userId = null, string userName = null, Dictionary<string, string> sorts = null, bool? exception = null, int pageIndex = 1, int pageSize = 10)
        {
            LogListModel logModel = new LogListModel()
            {
                From = from,
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
            return requestHelper.Post(baseUrl.TrimEnd('/') + "/log/getlistsimple?", logModel, null);
        }
        /// <summary>
        /// 获取日志对象列表的简单模式
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
        public ServiceModel<List<LogModel>> GetListSimple(string from = null, string controller = null, string action = null, DateTime? startTime = null, DateTime? endTime = null, string userId = null, string userName = null, Dictionary<string, string> sorts = null, bool? exception = null, int pageIndex = 1, int pageSize = 10)
        {
            var result = GetListSimpleJson(from, controller, action, startTime, endTime, userId, userName, sorts, exception, pageIndex, pageSize);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<LogModel>>>(result);
        }
        /// <summary>
        /// 通过id获取日志详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetByIdJson(string id)
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/getbyid/" + id, null);
        }
        /// <summary>
        /// 通过id获取日志详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ServiceModel<LogModel> GetById(string id)
        {
            var result = GetByIdJson(id);
            return JsonSerializerHelper.Deserialize<ServiceModel<LogModel>>(result);
        }
        /// <summary>
        /// 获取最新的日志,按天统计
        /// </summary>
        /// <param name="last">最近多少天</param>
        /// <returns></returns>
        public string GetOpRecordByDayJson(int last = 30)
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/recordbyday?last=" + last, null);
        }
        /// <summary>
        /// 获取最新的日志,按天统计
        /// </summary>
        /// <param name="last">最近多少天</param>
        /// <returns></returns>
        public ServiceModel<List<LogOpCountModel>> GetOpRecordByDay(int last = 30)
        {
            var result = GetOpRecordByDayJson(last);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<LogOpCountModel>>>(result);
        }
        /// <summary>
        /// 获取from列表
        /// </summary>
        /// <returns></returns>
        public string GetFromListJson()
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/getfromlist", null);
        }
        /// <summary>
        /// 获取from列表
        /// </summary>
        /// <returns></returns>
        public ServiceModel<List<FromCountModel>> GetFromList()
        {
            var result = GetFromListJson();
            return JsonSerializerHelper.Deserialize<ServiceModel<List<FromCountModel>>>(result);
        }
        /// <summary>
        /// 获取from中Controller列表
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public string GetControllersByFromJson(string from)
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/getcontrollersbyfrom?from=" + from, null);
        }
        /// <summary>
        /// 获取from中Controller列表
        /// </summary>
        /// <param name="from"></param>
        /// <returns></returns>
        public ServiceModel<List<ControllerCountModel>> GetControllersByFrom(string from)
        {
            var result = GetControllersByFromJson(from);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<ControllerCountModel>>>(result);
        }
        /// <summary>
        /// 获取from中action列表
        /// </summary>
        /// <param name="from"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public string GetActionsByControllerJson(string from, string controllerName)
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/getactionsbycontroller?from=" + from + "&controllerName=" + controllerName, null);
        }
        /// <summary>
        /// 获取from中action列表
        /// </summary>
        /// <param name="from"></param>
        /// <param name="controllerName"></param>
        /// <returns></returns>
        public ServiceModel<List<ActionCountModel>> GetActionsByController(string from, string controllerName)
        {
            var result = GetActionsByControllerJson(from, controllerName);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<ActionCountModel>>>(result);
        }
        /// <summary>
        /// 统计昨天,上月,全部日志
        /// </summary>
        /// <returns></returns>
        public string GetOperationsJson()
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/getoperations", null);
        }
        /// <summary>
        /// 统计昨天,上月,全部日志
        /// </summary>
        /// <returns></returns>
        public ServiceModel<OperationsCountModel> GetOperations()
        {
            var result = GetOperationsJson();
            return JsonSerializerHelper.Deserialize<ServiceModel<OperationsCountModel>>(result);
        }
    }
}
