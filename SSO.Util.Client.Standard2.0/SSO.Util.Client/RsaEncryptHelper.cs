using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;

namespace SSO.Util.Client
{
    /// <summary>
    /// rsa加密解密类
    /// </summary>
    public class RsaEncryptHelper
    {
        RSACryptoServiceProvider csp = new RSACryptoServiceProvider(1024);
        /// <summary>
        /// 加密,2048长度的秘钥能加密大概240个字符串,1024的110个字符串
        /// </summary>
        /// <param name="source">要加密的字符串</param>
        /// <param name="pubKey">公钥</param>
        /// <returns></returns>
        public string Encode(string source, string pubKey)
        {
            byte[] buffer = Convert.FromBase64String(pubKey);
            csp.ImportCspBlob(buffer);
            var bytesCypherText = csp.Encrypt(Encoding.UTF8.GetBytes(source), false);
            return Convert.ToBase64String(bytesCypherText);
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="secret">要解密的字符串</param>
        /// <param name="priKey">私钥</param>
        /// <returns></returns>
        public string Decode(string secret, string priKey)
        {
            byte[] buffer = Convert.FromBase64String(priKey);
            csp.ImportCspBlob(buffer);
            var bytesPlainTextData = csp.Decrypt(Convert.FromBase64String(secret), false);
            return Encoding.UTF8.GetString(bytesPlainTextData);
        }
        /// <summary>
        /// 公钥获取
        /// </summary>
        /// <returns></returns>
        public string GetPublicKey()
        {
            var pubKey = csp.ExportCspBlob(false);
            return Convert.ToBase64String(pubKey);
        }
        /// <summary>
        /// 私钥获取
        /// </summary>
        /// <returns></returns>
        public string GetPrivateKey()
        {
            var priKey = csp.ExportCspBlob(true);
            return Convert.ToBase64String(priKey);
        }

    }

}
