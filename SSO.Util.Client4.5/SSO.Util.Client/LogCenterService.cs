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
        public ServiceModel<string> Insert(string from, string controller, string action, string route, string querystring, string content, string userId, string userName, string userHost, string userAgent, int time = 0)
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
                CreateTime = DateTime.Now
            };
            var result = requestHelper.Post(baseUrl.TrimEnd('/') + "/log/insert", logModel, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        public string GetListJson(string from = null, string controller = null, string action = null, DateTime? startTime = null, DateTime? endTime = null, string userId = null, string userName = null, Dictionary<string, string> sorts = null, int pageIndex = 1, int pageSize = 10)
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
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return requestHelper.Post(baseUrl.TrimEnd('/') + "/log/getlist?", logModel, null);
        }
        public ServiceModel<List<LogModel>> GetList(string from = null, string controller = null, string action = null, DateTime? startTime = null, DateTime? endTime = null, string userId = null, string userName = null, Dictionary<string, string> sorts = null, int pageIndex = 1, int pageSize = 10)
        {
            var result = GetListJson(from, controller, action, startTime, endTime, userId, userName, sorts, pageIndex, pageSize);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<LogModel>>>(result);
        }
        public string GetListSimpleJson(string from = null, string controller = null, string action = null, DateTime? startTime = null, DateTime? endTime = null, string userId = null, string userName = null, Dictionary<string, string> sorts = null, int pageIndex = 1, int pageSize = 10)
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
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return requestHelper.Post(baseUrl.TrimEnd('/') + "/log/getlistsimple?", logModel, null);
        }
        public ServiceModel<List<LogModel>> GetListSimple(string from = null, string controller = null, string action = null, DateTime? startTime = null, DateTime? endTime = null, string userId = null, string userName = null, Dictionary<string, string> sorts = null, int pageIndex = 1, int pageSize = 10)
        {
            var result = GetListSimpleJson(from, controller, action, startTime, endTime, userId, userName, sorts, pageIndex, pageSize);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<LogModel>>>(result);
        }
        public string GetByIdJson(string id)
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/getbyid/" + id, null);
        }
        public ServiceModel<LogModel> GetById(string id)
        {
            var result = GetByIdJson(id);
            return JsonSerializerHelper.Deserialize<ServiceModel<LogModel>>(result);
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
        public string GetFromListJson()
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/getfromlist", null);
        }
        public ServiceModel<List<FromCountModel>> GetFromList()
        {
            var result = GetFromListJson();
            return JsonSerializerHelper.Deserialize<ServiceModel<List<FromCountModel>>>(result);
        }
        public string GetControllersByFromJson(string from)
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/getcontrollersbyfrom?from=" + from, null);
        }
        public ServiceModel<List<ControllerCountModel>> GetControllersByFrom(string from)
        {
            var result = GetControllersByFromJson(from);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<ControllerCountModel>>>(result);
        }
        public string GetActionsByControllerJson(string from, string controllerName)
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/getactionsbycontroller?from=" + from + "&controllerName=" + controllerName, null);
        }
        public ServiceModel<List<ActionCountModel>> GetActionsByController(string from, string controllerName)
        {
            var result = GetActionsByControllerJson(from, controllerName);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<ActionCountModel>>>(result);
        }
        public string GetOperationsJson()
        {
            return requestHelper.Get(baseUrl.TrimEnd('/') + "/log/getoperations", null);
        }
        public ServiceModel<OperationsCountModel> GetOperations()
        {
            var result = GetOperationsJson();
            return JsonSerializerHelper.Deserialize<ServiceModel<OperationsCountModel>>(result);
        }
    }
}
