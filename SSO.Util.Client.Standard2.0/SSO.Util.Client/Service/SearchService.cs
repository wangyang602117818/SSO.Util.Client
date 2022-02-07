using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSO.Util.Client
{
    /// <summary>
    /// 搜索服务类
    /// </summary>
    public class SearchService
    {
        private string baseUrl = "";
        HttpRequestHelper requestHelper = new HttpRequestHelper();
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="baseUrl">日志项目的url</param>
        public SearchService(string baseUrl)
        {
            this.baseUrl = baseUrl.TrimEnd('/');
        }
        /// <summary>
        /// 添加搜索数据
        /// </summary>
        /// <param name="database"></param>
        /// <param name="table"></param>
        /// <param name="key"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="doc_time">文档添加时间,不是进入ex时间)</param>
        /// <param name="extra"></param>
        /// <returns></returns>
        public ServiceModel<string> InsertSearchData(DataBaseType database, string table, string key, string title, string description, DateTime doc_time, string extra = "")
        {
            object data = new { operationType = OperationType.insert, database, table, key, title, description, doc_time, extra };
            var result = requestHelper.Post(baseUrl + "/searchData/insert", data, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        /// <summary>
        /// 删除搜索数据
        /// </summary>
        /// <param name="database"></param>
        /// <param name="table"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public ServiceModel<string> DeleteSearchData(DataBaseType database, string table, string key)
        {
            object data = new { operationType = OperationType.delete, database, table, key };
            var result = requestHelper.Post(baseUrl + "/searchData/insert", data, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        /// <summary>
        /// 根据前缀建议
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public ServiceModel<List<SuggestData>> Suggest(string word)
        {
            var result = requestHelper.Get(baseUrl + "/searchdata/suggest?word=" + word, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<SuggestData>>>(result);
        }
        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="word"></param>
        /// <param name="highlight"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ServiceModel<List<SearchData>> Search(string word, bool highlight = false, int pageIndex = 1, int pageSize = 10)
        {
            string result = requestHelper.Get(baseUrl + "/searchdata/search?word=" + word + "&highlight=" + highlight + "&pageIndex=" + pageIndex + "&pageSize=" + pageSize, null);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<SearchData>>>(result);
        }
    }
    /// <summary>
    /// search返回类
    /// </summary>
    public class SearchData
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public DateTime doc_time { get; set; }
        public DateTime create_time { get; set; }
    }
    /// <summary>
    /// suggest返回类
    /// </summary>
    public class SuggestData
    {
        [JsonProperty("_id")]
        public string id { get; set; }
        public string text { get; set; }
    }
}
