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
    /// 对称加密算法
    /// </summary>
    public static class SymmetricEncryptHelper
    {
        //初始化向量
        private static byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        /// <summary>
        /// Aes加密算法，替代des
        /// </summary>
        /// <param name="sourceString">待加密字符串</param>
        /// <param name="key">base64形式的key,256位。如果为明文字符串则长度只能是：16/24/32</param>
        /// <param name="isBase64Key">是否是base64形式的key</param>
        /// <returns></returns>
        public static string AesEncode(string sourceString, string key, bool isBase64Key = true)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(sourceString);
            return Convert.ToBase64String(AesEncode(new MemoryStream(bytes), key, isBase64Key).ToArray());
        }

        public static MemoryStream AesEncode(Stream sourceStream, string key, bool isBase64Key = true)
        {
            byte[] keyBytes = isBase64Key ? key.Base64StrToBuffer() : key.StrToBuffer();
            using (Rijndael rijndael = Rijndael.Create())
            {
                rijndael.Mode = CipherMode.ECB;  //ecb模式下跟IV没有关系
                using (ICryptoTransform transform = rijndael.CreateEncryptor(keyBytes, IV))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
                    sourceStream.Position = 0;
                    sourceStream.CopyTo(cryptoStream); //往memoryStream中写入流
                    cryptoStream.FlushFinalBlock();
                    return memoryStream;

                }
            }
        }
        /// <summary>
        /// Aes解密算法
        /// </summary>
        /// <param name="secretString">密文</param>
        /// <param name="key">base64形式的key，256位</param>
        /// <param name="isBase64Key">是否是base64形式的key</param>
        /// <returns></returns>
        public static string AesDecode(string secretString, string key, bool isBase64Key = true)
        {
            byte[] bytes = Convert.FromBase64String(secretString);
            return AesDecode(new MemoryStream(bytes), key, isBase64Key).ToStr();
        }

        public static MemoryStream AesDecode(Stream sourceStream, string key, bool isBase64Key = true)
        {
            byte[] keyBytes = isBase64Key ? key.Base64StrToBuffer() : key.StrToBuffer();
            using (Rijndael rijndael = Rijndael.Create())
            {
                rijndael.Mode = CipherMode.ECB;
                using (ICryptoTransform transform = rijndael.CreateDecryptor(keyBytes, IV)) //创建一个解密器
                {
                    MemoryStream memoryStream = new MemoryStream();
                    CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
                    sourceStream.Position = 0;
                    sourceStream.CopyTo(cryptoStream);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream;

                }
            }
        }
        /// <summary>
        /// DES加密，8个长度的字符串秘钥（64位，其中第8、16、24、32、40、48、56、64位是校验位）,校验位不参与密码计算
        /// 所以可能会出现，不同的key加密出来的字符串是一样的，不同的key可以解密相同的字符串
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="key">字符串长度为8的key</param>
        /// <returns>返回base64编码的密文</returns>
        public static string DESEncode(string sourceString, string key)
        {
            byte[] keyBytes = Convert.FromBase64String(key);
            byte[] valueBytes = Encoding.UTF8.GetBytes(sourceString);
            using (DES des = DES.Create())
            {
                des.Mode = CipherMode.ECB;
                using (ICryptoTransform transform = des.CreateEncryptor(keyBytes, IV))  //创建一个加密器
                {
                    MemoryStream memoryStream = new MemoryStream();
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(valueBytes, 0, valueBytes.Length); //往memoryStream中写入流
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }
        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="secretString">base64加密字符串</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DESDecode(string secretString, string key)
        {
            byte[] keyBytes = Convert.FromBase64String(key);
            byte[] inputBytes = Convert.FromBase64String(secretString);
            using (DES des = DES.Create())
            {
                des.Mode = CipherMode.ECB;
                using (ICryptoTransform transform = des.CreateDecryptor(keyBytes, IV))  //创建一个解密器
                {
                    MemoryStream memoryStream = new MemoryStream();
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        return Encoding.UTF8.GetString(memoryStream.ToArray());
                    }
                }
            }
        }
        /// <summary>
        /// 3DES加密，为DES向AES过度的版本
        /// </summary>
        /// <param name="sourceString">要加密的字符串</param>
        /// <param name="key">base64形式的key</param>
        /// <returns>返回base64编码的密文</returns>
        public static string TripleDESEncode(string sourceString, string key)
        {
            byte[] keyBytes = Convert.FromBase64String(key);
            byte[] valueBytes = Encoding.UTF8.GetBytes(sourceString);
            using (TripleDES tripleDes = TripleDES.Create())
            {
                tripleDes.Mode = CipherMode.ECB; //设置后初始化向量失效
                using (ICryptoTransform transform = tripleDes.CreateEncryptor(keyBytes, IV))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(valueBytes, 0, valueBytes.Length); //往memoryStream中写入流
                        cryptoStream.FlushFinalBlock();
                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }
        /// <summary>
        /// 3DES解密
        /// </summary>
        /// <param name="secretString">密文</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string TripleDESDecode(string secretString, string key)
        {
            byte[] keyBytes = Convert.FromBase64String(key);
            byte[] inputBytes = Convert.FromBase64String(secretString);
            using (TripleDES tripleDes = TripleDES.Create())
            {
                tripleDes.Mode = CipherMode.ECB;
                using (ICryptoTransform transform = tripleDes.CreateDecryptor(keyBytes, IV))  //创建一个解密器
                {
                    MemoryStream memoryStream = new MemoryStream();
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(inputBytes, 0, inputBytes.Length);
                        cryptoStream.FlushFinalBlock();
                        return Encoding.UTF8.GetString(memoryStream.ToArray());
                    }
                }
            }
        }
        /// <summary>
        /// 生成DES加密密钥
        /// </summary>
        /// <returns></returns>
        public static string GenerateDESKey
        {
            get { return Convert.ToBase64String(DES.Create().Key); }
        }
        /// <summary>
        /// 生成TripleDES加密密钥
        /// </summary>
        public static string GenerateTripleDESKey
        {
            get { return Convert.ToBase64String(TripleDES.Create().Key); }
        }
        /// <summary>
        /// 生成256位的AES加密密钥
        /// </summary>
        public static string GenerateAESKey
        {
            get { return Convert.ToBase64String(Rijndael.Create().Key); }
        }

    }
}
