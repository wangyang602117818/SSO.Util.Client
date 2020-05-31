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
    public static class StringExtention
    {
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
        public static string GetSha256(this string str)
        {
            byte[] SHA256Data = Encoding.UTF8.GetBytes(str);
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] by = Sha256.ComputeHash(SHA256Data);
            return BitConverter.ToString(by).Replace("-", "").ToLower();
        }
        public static string ToStr(this Stream stream)
        {
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }
        public static Stream ToStream(this string str)
        {
            byte[] array = Encoding.UTF8.GetBytes(str);
            MemoryStream stream = new MemoryStream(array);
            stream.Position = 0;
            return stream;
        }
        public static byte[] ToBytes(this Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }
        public static string GetMachineName(this string str)
        {
            Match match = Regex.Match(str, @"\\\\(.+?)\\");
            return match.Groups[1].Value;
        }
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
        public static string GetFileName(this string str)
        {
            var index = str.LastIndexOf("\\");
            return str.Substring(index + 1);
        }
        public static string GetFileExt(this string str)
        {
            var index = str.LastIndexOf(".");
            if (index == -1) return "";
            return str.Substring(index);
        }
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }
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
        /// <param name="toJsonStr"></param>
        /// <returns></returns>
        public static string ReplaceJsonString(this string str)
        {
            str = new Regex("ISODate\\(\"(.*?)\"\\)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Replace(str, JsonReplacement);
            str = new Regex("ObjectId\\(\"(.*?)\"\\)", RegexOptions.IgnoreCase | RegexOptions.Multiline).Replace(str, "\"$1\"");
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
            return "\"" + match.Groups[1].Value.MilliTimeStampToDateTime().ToLocalTime().ToString(AppSettings.DateTimeFormat) + "\"";
        }
    }
}
