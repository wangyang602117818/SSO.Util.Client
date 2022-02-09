using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 下载文件的描述
    /// </summary>
    public class DownloadFileItem
    {
        /// <summary>
        /// 文件描述
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// ContentType类型
        /// </summary>
        public string ContentType { get; set; }
        /// <summary>
        /// Content长度
        /// </summary>
        public long ContentLength { get; set; }
        /// <summary>
        /// 文件流
        /// </summary>
        public Stream FileStream { get; set; }
    }
}
