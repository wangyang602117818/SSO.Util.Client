using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 搜索服务类
    /// </summary>
    public class SearchService
    {
        /// <summary>
        /// 消息中心地址
        /// </summary>
        public string baseUrl = "";
        /// <summary>
        /// jwt token
        /// </summary>
        public string Token { get; set; }
        HttpRequestHelper requestHelper = new HttpRequestHelper();
        Dictionary<string, string> headers = new Dictionary<string, string>();
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="baseUrl">消息中心项目的基本url</param>
        public SearchService(string baseUrl)
        {
            this.baseUrl = baseUrl.TrimEnd('/');
        }
        /// <summary>
        /// 添加搜索数据
        /// </summary>
        /// <param name="database">数据库</param>
        /// <param name="table">表</param>
        /// <param name="key">唯一id</param>
        /// <param name="title">搜索标题</param>
        /// <param name="description">搜索内容</param>
        /// <param name="doc_time">文档添加时间,不是进入es时间)</param>
        /// <param name="extra">额外信息</param>
        /// <returns></returns>
        public ServiceModel<string> InsertSearchData(DataBaseType database, string table, string key, string title, string description, DateTime doc_time, string extra = "")
        {
            object data = new { operationType = OperationType.insert, database, table, key, title, description, doc_time, extra };
            var result = requestHelper.Post(baseUrl + "/searchData/insert", data, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        /// <summary>
        /// 删除搜索数据
        /// </summary>
        /// <param name="database">数据库</param>
        /// <param name="table">表</param>
        /// <param name="key">唯一id</param>
        /// <returns></returns>
        public ServiceModel<string> DeleteSearchData(DataBaseType database, string table, string key)
        {
            object data = new { operationType = OperationType.delete, database, table, key };
            var result = requestHelper.Post(baseUrl + "/searchData/insert", data, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        /// <summary>
        /// 根据前缀建议
        /// </summary>
        /// <param name="word">关键词</param>
        /// <param name="database">数据库</param>
        /// <param name="table">表</param>
        /// <returns></returns>
        public ServiceModel<List<SuggestData>> Suggest(string word, DataBaseType database = DataBaseType.none, string table = "")
        {
            var url = baseUrl + "/searchdata/suggest?word=" + word;
            if (database != DataBaseType.none) url += "&database=" + database.ToString();
            if (!table.IsNullOrEmpty()) url += "&table=" + table.ToString();
            var result = requestHelper.Get(url, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<SuggestData>>>(result);
        }
        /// <summary>
        /// 搜索
        /// </summary>
        /// <param name="word">关键词</param>
        /// <param name="database">数据库</param>
        /// <param name="table">表</param>
        /// <param name="highlight">结果是否高亮</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ServiceModel<List<SearchData>> Search(string word, DataBaseType database = DataBaseType.none, string table = "", bool highlight = false, int pageIndex = 1, int pageSize = 10)
        {
            string url = baseUrl + "/searchdata/search?word=" + word + "&highlight=" + highlight + "&pageIndex=" + pageIndex + "&pageSize=" + pageSize;
            if (database != DataBaseType.none) url += "&database=" + database.ToString();
            if (!table.IsNullOrEmpty()) url += "&table=" + table.ToString();
            string result = requestHelper.Get(url, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<SearchData>>>(result);
        }
    }
    /// <summary>
    /// search返回类
    /// </summary>
    public class SearchData
    {
        public string database { get; set; }
        public string table { get; set; }
        public string key { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string extra { get; set; }
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
