using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    public static class StreamExtention
    {
        /// <summary>
        /// 获取文件的MD5码
        /// </summary>
        /// <returns></returns>
        public static string GetMD5(this Stream fileStream)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fileStream);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            fileStream.Position = 0;
            return sb.ToString();
        }
        public static string GetMD5(this byte[] bytes)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        public static string GetSha256(this Stream fileStream)
        {
            SHA256Managed Sha256 = new SHA256Managed();
            byte[] by = Sha256.ComputeHash(fileStream);
            return BitConverter.ToString(by).Replace("-", "").ToLower();
        }
    }
}
