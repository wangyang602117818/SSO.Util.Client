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
    }
}
