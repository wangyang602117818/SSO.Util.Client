using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    /// <summary>
    /// 对称加密解密类
    /// </summary>
    public static class AesEncryptHelper
    {
        //初始化向量
        private static byte[] IV = { 137, 73, 153, 109, 30, 161, 2, 250, 37, 120, 158, 116, 24, 217, 227, 250 };
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="sourceString">要加密的字符串</param>
        /// <param name="key">秘钥</param>
        /// <returns></returns>
        public static string Encode(string sourceString, string key)
        {
            // Check arguments.
            if (sourceString == null || sourceString.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key.Base64StrToBuffer();
                aesAlg.IV = IV;
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(sourceString);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encrypted);
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="secretString">加密后的字符串,base64形式</param>
        /// <param name="key">秘钥</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Decode(string secretString, string key)
        {
            // Check arguments.
            if (secretString == null || secretString.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            string plaintext = null;
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key.Base64StrToBuffer();
                aesAlg.IV = IV;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(secretString)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
            return plaintext;
        }
        public static string GenerateAESKey()
        {
            using (Aes aesAlg = Aes.Create())
            {
                return Convert.ToBase64String(aesAlg.Key);
            }
        }
    }
}
