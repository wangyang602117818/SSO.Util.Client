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
            RemoteUrl = remoteUrl;
            Token = token;
        }
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="contentType">文件contentType</param>
        /// <param name="stream">文件流</param>
        /// <returns></returns>
        public ServiceModel<FileResponse> Upload(string fileName, string contentType, Stream stream)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", Token);
            string result = requestHelper.PostFile(RemoteUrl + "/upload/file", "files", fileName, contentType, stream, null, headers);
            ServiceModel<List<FileResponse>> response = JsonSerializerHelper.Deserialize<ServiceModel<List<FileResponse>>>(result);
            return new ServiceModel<FileResponse>()
            {
                code = response.code,
                message = response.message,
                result = response.result[0],
                count = response.count
            };
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
    }
}
