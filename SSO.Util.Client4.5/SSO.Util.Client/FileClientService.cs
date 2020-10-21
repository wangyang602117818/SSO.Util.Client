using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 文件服务类
    /// </summary>
    public class FileClientService
    {
        private HttpRequestHelper requestHelper = new HttpRequestHelper();
        /// <summary>
        /// 文件服务器的url
        /// </summary>
        public string RemoteUrl { get; set; }
        /// <summary>
        /// jwt token
        /// </summary>
        public string Token { get; set; }
        /// <summary>
        /// 文件服务器的url
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <param name="token"></param>
        public FileClientService(string remoteUrl, string token)
        {
            RemoteUrl = remoteUrl.TrimEnd('/');
            Token = token;
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentType">文件contentType</param>
        /// <param name="stream">文件流</param>
        /// <param name="param">其他参数</param>
        /// <returns></returns>
        public ServiceModel<FileResponse> Upload(string fileName, string contentType, Stream stream, Dictionary<string, string> param = null)
        {
            List<UploadFileItem> files = new List<UploadFileItem>();
            files.Add(new UploadFileItem() { FileName = fileName, ContentType = contentType, FileStream = stream });
            var result = Uploads(files, param);
            return new ServiceModel<FileResponse>()
            {
                code = result.code,
                message = result.message,
                result = result.result[0],
                count = result.count
            };
        }
        /// <summary>
        /// 上传多个文件
        /// </summary>
        /// <param name="files"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public ServiceModel<List<FileResponse>> Uploads(IEnumerable<UploadFileItem> files, Dictionary<string, string> param = null)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", Token);
            string result = requestHelper.PostFile(RemoteUrl + "/upload/file", files, param, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<FileResponse>>>(result);
        }
        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="id">文件id</param>
        /// <param name="filename">文件名称</param>
        /// <param name="flag">文件flag,图片才需要</param>
        /// <returns></returns>
        public DownloadFileItem DownloadFile(string id, string filename, string flag = "")
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", Token);
            return requestHelper.GetFile(RemoteUrl + "/file/" + id + "/" + filename + "?mode=download&flag=" + flag, headers);
        }
        /// <summary>
        /// 下载文件图标
        /// </summary>
        /// <param name="id">文件id</param>
        /// <param name="filename">文件名称</param>
        /// <returns></returns>
        public DownloadFileItem DownloadFileIcon(string id, string filename)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", Token);
            return requestHelper.GetFile(RemoteUrl + "/file/GetFileIconWrapId/" + id + filename.GetFileExt(), headers);
        }
        /// <summary>
        /// 获取文件转换进度
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ServiceModel<int> FileState(string id)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", Token);
            string state = requestHelper.Get(RemoteUrl + "/data/FileState/" + id, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<int>>(state);
        }
        /// <summary>
        /// 获取文件列表
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="from"></param>
        /// <param name="filter"></param>
        /// <param name="fileType"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="sorts"></param>
        /// <param name="delete"></param>
        /// <returns></returns>
        public ServiceModel<List<FileItem>> GetFileList(int pageIndex = 1, int pageSize = 10, string from = "", string filter = "", string fileType = "", DateTime? startTime = null, DateTime? endTime = null, Dictionary<string, string> sorts = null, bool delete = false)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", Token);
            var url = RemoteUrl + "/data/GetFiles?pageIndex=" + pageIndex + "&pageSize=" + pageSize;
            if (!from.IsNullOrEmpty()) url += "&from=" + from;
            if (!filter.IsNullOrEmpty()) url += "&filter=" + filter;
            if (!fileType.IsNullOrEmpty()) url += "&fileType=" + fileType;
            if (startTime != null) url += "&startTime=" + startTime.Value.ToString(AppSettings.DateTimeFormat);
            if (endTime != null) url += "&endTime=" + endTime.Value.ToString(AppSettings.DateTimeFormat);
            var index = 0;
            if (sorts != null)
            {
                foreach (var item in sorts)
                {
                    var key = item.Key;
                    var value = item.Value;
                    url += "&sorts[" + index + "].key=" + key;
                    url += "&sorts[" + index + "].value=" + value;
                    index++;
                }
            }
            url += "&delete=" + delete;
            string list = requestHelper.Get(url, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<FileItem>>>(list);
        }
        /// <summary>
        /// 获取from列表
        /// </summary>
        /// <returns></returns>
        public ServiceModel<List<string>> GetFromList()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", Token);
            var url = RemoteUrl + "/data/GetFromList";
            string list = requestHelper.Get(url, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<string>>>(list);
        }
        /// <summary>
        /// 获取ExtensionMap
        /// </summary>
        /// <returns></returns>
        public ServiceModel<List<ExtensionMap>> GetExtensionMap()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", Token);
            var url = RemoteUrl + "/data/GetExtensionsMap";
            string list = requestHelper.Get(url, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<ExtensionMap>>>(list);
        }
    }
}
