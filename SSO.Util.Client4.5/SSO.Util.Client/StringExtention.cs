using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 字符串扩展类
    /// </summary>
    public static class StringExtention
    {
        /// <summary>
        /// 字符串转md5
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToMD5(this string str)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] md5bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in md5bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// 字符串转sha256
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetSha256(this string str)
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(str);
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] by = Sha256.ComputeHash(SHA256Data);
            return BitConverter.ToString(by).Replace("-", "").ToLower();
        }
        /// <summary>
        /// 文件流转字符串
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string ToStr(this Stream stream)
        {
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
        /// <summary>
        /// 字符串转文件流
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Stream ToStream(this string str)
        {
            byte[] array = Encoding.UTF8.GetBytes(str);
            MemoryStream stream = new MemoryStream(array);
            stream.Position = 0;
            return stream;
        }
        /// <summary>
        /// 文件流转字节数组
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ToBytes(this Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetMachineName(this string str)
        {
            Match match = Regex.Match(str, @"\\\\(.+?)\\");
            return match.Groups[1].Value;
        }
        /// <summary>
        /// 移除字符串中的html标签
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string RemoveHtml(this string str)
        {
            return Regex.Replace(str, "<[^>]+>", "").Replace("&[^;]+;", "");
        }
        /// <summary>
        /// 字符串转UTF8字节数组
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] StrToBuffer(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
        /// <summary>
        /// base64字符串转UTF8字节数组
        /// </summary>
        /// <param name="base64Str"></param>
        /// <returns></returns>
        public static byte[] Base64StrToBuffer(this string base64Str)
        {
            return Convert.FromBase64String(base64Str);
        }
        /// <summary>
        /// 获取路径中的文件名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetFileName(this string str)
        {
            var index = str.LastIndexOf("\\");
            return str.Substring(index + 1);
        }
        /// <summary>
        /// 获取文件的扩展名
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string GetFileExt(this string str)
        {
            var index = str.LastIndexOf(".");
            if (index == -1) return "";
            return str.Substring(index);
        }
        /// <summary>
        /// IsNullOrEmpty封装
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
        /// <summary>
        /// 格式化成2位的日期
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        public static string FormatMonth(this int month)
        {
            return month < 10 ? "0" + month : month.ToString();
        }
        /// <summary>
        /// string 转成 url 安全的base64 编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StrToBase64(this string str)
        {
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
            return Base64SecureURL.Encode(base64);
        }
        /// <summary>
        /// url 安全的base64 编码 转 string
        /// </summary>
        /// <returns></returns>
        public static string Base64ToStr(this string base64)
        {
            base64 = Base64SecureURL.Decode(base64);
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
        /// <summary>
        /// 替换由 ToJson() 转换的json字符串 中 ObjectId("") 和 ISODate("2020-05-30T08:50:10.048Z") 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceJsonString(this string str)
        {
            str = new Regex("ISODate\\(\"(.*?)\"\\)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Replace(str, JsonReplacement);
            str = new Regex("ObjectId\\(\"(.*?)\"\\)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Replace(str, "\"$1\"");
            str = new Regex("NumberLong\\((.*?)\\)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Replace(str, "\"$1\"");
            return str;
        }
        private static string JsonReplacement(Match match)
        {
            var time = match.Groups[1].Value.Replace("Z", "").Replace("T", " ");
            return "\"" + DateTime.Parse(time).ToLocalTime().ToString(AppSettings.DateTimeFormat) + "\"";
        }
        /// <summary>
        /// 替换由 .ToJson(new JsonWriterSettings() { OutputMode = JsonOutputMode.Strict }) 转换的json字符串 中{"$date": ""} 和{"$oid": ""}
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceStrictJsonString(this string str)
        {
            str = new Regex("{\\s+\"\\$date\".+?(\\w{13}).+?}", RegexOptions.IgnoreCase | RegexOptions.Multiline).Replace(str, StrictJsonReplacement);
            str = new Regex("{\\s+\"\\$oid\".+?(\\w{24}).+?}", RegexOptions.IgnoreCase | RegexOptions.Multiline).Replace(str, "\"$1\"");
            return str;
        }
        private static string StrictJsonReplacement(Match match)
        {
            return "\"" + match.Groups[1].Value.TimeStampToDateTime().ToString(AppSettings.DateTimeFormat) + "\"";
        }
        /// <summary>
        /// 替换 http://www. 或者 https://www. 或者 www. 为""
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ReplaceHttpPrefix(this string str)
        {
            return new Regex("https?://|www.", RegexOptions.IgnoreCase).Replace(str, "");
        }
        /// <summary>
        /// 把驼峰形式的参数转换成_形式
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string PascalToUnderline(this string name)
        {
            string str = "";
            for (int j = 0; j < name.Length; j++)  //strItem是原始字符串
            {
                char c = name[j];
                str += (c >= 'A' && c <= 'Z') ? ("_" + c.ToString().ToLower()) : c.ToString();
            }
            return str.TrimStart('_');
        }
        /// <summary>
        /// 判断object中的类型是否的值类型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool ObjectIsValueType(this object obj)
        {
            //全部数值类型
            if (obj is sbyte) return true;
            if (obj is byte) return true;
            if (obj is short) return true;
            if (obj is ushort) return true;
            if (obj is int) return true;
            if (obj is uint) return true;
            if (obj is long) return true;
            if (obj is ulong) return true;
            //浮点类型
            if (obj is float) return true;
            if (obj is double) return true;
            if (obj is decimal) return true;
            //bool类型
            if (obj is bool) return true;
            //字符串类型
            if (obj is char) return true;
            if (obj is string) return true;
            //时间类型
            if (obj is DateTime) return true;
            if (obj is DateTimeOffset) return true;
            return false;
        }
        /// <summary>
        /// 判断是否为数字
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsNumeric(this string value)
        {
            return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
        }
    }
}
