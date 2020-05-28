using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UtilToolkit
{
    /// <summary>
    /// Hash散列算法
    /// </summary>
    public static class HashEncryptHelper
    {
        /// <summary>
        /// 字符串md5计算,不可逆，生成32位字符串
        /// </summary>
        /// <param name="str">要计算的字符串</param>
        /// <returns></returns>
        public static string StringMd5(string str)
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
        /// 文件MD5计算
        /// </summary>
        /// <param name="path">要计算的文件的本地路径</param>
        /// <returns></returns>
        public static string FileMd5(string path)
        {
            if (!File.Exists(path)) return "";
            using (FileStream fs = File.Open(path, FileMode.Open))
            {
                return FileMd5(fs);
            }
        }
        /// <summary>
        /// 文件MD5计算
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <returns></returns>
        public static string FileMd5(Stream stream)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] md5Bytes = md5.ComputeHash(stream);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in md5Bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
        /// <summary>
        /// SHA1加密，不可逆，生成40位字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string StringSHA1(string str)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] sha1bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in sha1bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
