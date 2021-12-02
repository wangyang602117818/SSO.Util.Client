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
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ServiceModel<List<SearchData>> Search(string word, int pageIndex = 1, int pageSize = 10)
        {
            string result = requestHelper.Get(baseUrl + "/searchdata/search?word=" + word + "&pageIndex=" + pageIndex + "&pageSize=" + pageSize, null);
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
