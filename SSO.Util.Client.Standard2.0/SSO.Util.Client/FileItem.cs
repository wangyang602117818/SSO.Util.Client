using System;
using System.Collections.Generic;
using System.Text;

namespace SSO.Util.Client
{
    /// <summary>
    /// 文件类
    /// </summary>
    public class FileItem
    {
        public string _id { get; set; }
        public string From { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public string Machine { get; set; }
        public string Folder { get; set; }
        public string Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Download { get; set; }
        public string FileType { get; set; }
        public string ContentType { get; set; }
        public int Duration { get; set; }
        public string Owner { get; set; }
        public int State { get; set; }
        public int Percent { get; set; }
        public int ProcessCount { get; set; }
        public bool Delete { get; set; }
        public bool Exception { get; set; }
        public DateTime? DeleteTime { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}
