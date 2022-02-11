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
        Dictionary<string, string> headers = new Dictionary<string, string>();
        /// <summary>
        /// 文件服务器的url
        /// </summary>
        /// <param name="remoteUrl"></param>
        /// <param name="token"></param>
        public FileClientService(string remoteUrl, string token)
        {
            RemoteUrl = remoteUrl.TrimEnd('/');
            Token = token;
            headers.Add("Authorization", Token);
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentType">文件contentType</param>
        /// <param name="stream">文件流</param>
        /// <param name="roles">角色设置</param>
        /// <param name="users">用户设置</param>
        /// <returns></returns>
        public ServiceModel<FileResponse> Upload(string fileName, string contentType, Stream stream, IEnumerable<string> roles = null, IEnumerable<string> users = null)
        {
            List<UploadFileItem> files = new List<UploadFileItem>();
            files.Add(new UploadFileItem() { FileName = fileName, ContentType = contentType, FileStream = stream });
            var result = Uploads(files, roles, users);
            return new ServiceModel<FileResponse>()
            {
                code = result.code,
                message = result.message,
                result = result.result?[0],
                count = result.count
            };
        }
        /// <summary>
        /// 上传多个文件
        /// </summary>
        /// <param name="files">文件列表</param>
        /// <param name="roles">角色设置</param>
        /// <param name="users">用户设置</param>
        /// <returns></returns>
        public ServiceModel<List<FileResponse>> Uploads(IEnumerable<UploadFileItem> files, IEnumerable<string> roles = null, IEnumerable<string> users = null)
        {
            Dictionary<string, string> param = new Dictionary<string, string>() { };
            if (roles != null && roles.Count() > 0) param.Add("roles", JsonSerializerHelper.Serialize(roles));
            if (users != null && users.Count() > 0) param.Add("users", JsonSerializerHelper.Serialize(users));
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
            return requestHelper.GetFile(RemoteUrl + "/file/GetFileIconWrapId/" + id + filename.GetFileExt(), headers);
        }
        /// <summary>
        /// 获取文件转换进度
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ServiceModel<int> FileState(string id)
        {
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
        public ServiceModel<List<FileItem>> GetFileList(int pageIndex = 1, int pageSize = 10, string from = "", string filter = "", FileType fileType = FileType.all, DateTime? startTime = null, DateTime? endTime = null, Dictionary<string, string> sorts = null, bool delete = false)
        {
            var url = RemoteUrl + "/data/GetFiles?pageIndex=" + pageIndex + "&pageSize=" + pageSize;
            if (!from.IsNullOrEmpty()) url += "&from=" + from;
            if (!filter.IsNullOrEmpty()) url += "&filter=" + filter;
            if (fileType != FileType.all) url += "&fileType=" + fileType.ToString();
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
        /// 获取文件详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ServiceModel<FileItem> GetFileInfo(string id)
        {
            var url = RemoteUrl + "/data/GetFileInfo/" + id;
            string item = requestHelper.Get(url, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<FileItem>>(item);
        }
        /// <summary>
        /// 获取文件详情列表
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public ServiceModel<List<FileItem>> GetFileInfos(IEnumerable<string> ids)
        {
            var url = RemoteUrl + "/data/GetFileInfos";
            string result = requestHelper.Post(url, new { Ids = ids }, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<FileItem>>>(result);
        }
        /// <summary>
        /// 获取from列表
        /// </summary>
        /// <returns></returns>
        public ServiceModel<List<FromData>> GetFromList()
        {
            var url = RemoteUrl + "/data/GetFromList";
            string list = requestHelper.Get(url, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<FromData>>>(list);
        }
        /// <summary>
        /// 获取ExtensionMap
        /// </summary>
        /// <returns></returns>
        public ServiceModel<List<ExtensionMap>> GetExtensionMap()
        {
            var url = RemoteUrl + "/data/GetExtensionsMap";
            string list = requestHelper.Get(url, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<List<ExtensionMap>>>(list);
        }
        /// <summary>
        /// 移除文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public ServiceModel<string> RemoveFile(string fileId)
        {
            var url = RemoteUrl + "/data/Remove/" + fileId;
            string result = requestHelper.Get(url, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        /// <summary>
        /// 批量移除文件
        /// </summary>
        /// <param name="fileIds"></param>
        /// <returns></returns>
        public ServiceModel<string> RemoveFiles(IEnumerable<string> fileIds)
        {
            var url = RemoteUrl + "/data/Removes";
            string result = requestHelper.Post(url, new { Ids = fileIds }, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        /// <summary>
        /// 恢复文件
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        public ServiceModel<string> RestoreFile(string fileId)
        {
            var url = RemoteUrl + "/data/Restore/" + fileId;
            string result = requestHelper.Get(url, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        /// <summary>
        /// 批量恢复文件
        /// </summary>
        /// <param name="fileIds"></param>
        /// <returns></returns>
        public ServiceModel<string> RestoreFiles(IEnumerable<string> fileIds)
        {
            var url = RemoteUrl + "/data/Restores";
            string result = requestHelper.Post(url, new { Ids = fileIds }, headers);
            return JsonSerializerHelper.Deserialize<ServiceModel<string>>(result);
        }
        /// <summary>
        /// 获取多种流m3u8清单文件
        /// </summary>
        /// <param name="id">原文件id</param>
        /// <param name="filename">文件名+".m3u"</param>
        /// <param name="time">记录视频播放时间(s)</param>
        /// <returns></returns>
        public DownloadFileItem M3u8MultiStream(string id, string filename, int time = 0)
        {
            if (time>0) headers.Add("time", time.ToString());
            return requestHelper.GetFile(RemoteUrl + "/file/" + id + "/" + filename, headers);
        }
        /// <summary>
        /// 获取单流m3u8清单文件
        /// </summary>
        /// <param name="id">原文件id</param>
        /// <param name="filename">子文件id+".m3u8"</param>
        /// <param name="time">记录视频播放时间(s)</param>
        /// <returns></returns>
        public DownloadFileItem M3u8(string id, string filename, int time = 0)
        {
            if (time > 0) headers.Add("time", time.ToString());
            return requestHelper.GetFile(RemoteUrl + "/file/" + id + "/" + filename, headers);
        }
        /// <summary>
        /// 获取切片文件
        /// </summary>
        /// <param name="id">原文件id</param>
        /// <param name="filename">子文件id+".ts"</param>
        /// <returns></returns>
        public DownloadFileItem Ts(string id, string filename)
        {
            return requestHelper.GetFile(RemoteUrl + "/file/" + id + "/" + filename, headers);
        }
    }
}
