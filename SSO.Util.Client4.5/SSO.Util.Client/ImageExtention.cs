﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSO.Util.Client
{
    public static class ImageExtention
    {
        public static ImageFormat GetImageFormat(string ext)
        {
            switch (ext.ToLower())
            {
                case ".jpg":
                    return ImageFormat.Jpeg;
                case ".png":
                    return ImageFormat.Png;
                case ".gif":
                    return ImageFormat.Gif;
                case ".bmp":
                    return ImageFormat.Bmp;
                case ".jpeg":
                    return ImageFormat.Jpeg;
                case ".ico":
                    return ImageFormat.Icon;
                case ".tif":
                    return ImageFormat.Tiff;
            }
            return ImageFormat.Jpeg;
        }
        public static string GetContentType(byte[] buffer)
        {
            switch (GetImageType(buffer).ToLower())
            {
                case "jpg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                case "gif":
                    return "image/gif";
                case "bmp":
                    return "application/x-bmp";
                case "jpeg":
                    return "image/jpeg";
                case "pic":
                    return "application/x-pic";
                case "ico":
                    return "image/x-icon";
                case "tif":
                    return "image/tiff";
                case "svg":
                    return "image/svg+xml";
            }
            return "image/*";
        }
        public static string GetImageType(Stream stream)
        {
            string header = GetHeaderInfo(stream).ToUpper();
            return GetImageTypeFromHeader(header);
        }
        public static string GetImageType(byte[] buffer)
        {
            string header = GetHeaderInfo(buffer).ToUpper();
            return GetImageTypeFromHeader(header);
        }
        public static string GetImageTypeFromHeader(string header)
        {
            if (header.StartsWith("FFD8FF"))
            {
                return "JPG";
            }
            else if (header.StartsWith("49492A"))
            {
                return "TIFF";
            }
            else if (header.StartsWith("424D"))
            {
                return "BMP";
            }
            else if (header.StartsWith("474946"))
            {
                return "GIF";
            }
            else if (header.StartsWith("89504E470D0A1A0A"))
            {
                return "PNG";
            }
            else if (header.StartsWith("3C3F786D6C"))
            {
                return "XML";
            }
            else
            {
                return "";
            }
        }
        public static string GetHeaderInfo(byte[] buffer)
        {
            StringBuilder sb = new StringBuilder();
            for (var i = 0; i < 8; i++)
            {
                sb.Append(buffer[i].ToString("X2"));
            }
            return sb.ToString();
        }
        public static string GetHeaderInfo(Stream stream)
        {
            byte[] buffer = new byte[8];
            BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true);
            reader.Read(buffer, 0, buffer.Length);
            reader.Close();
            StringBuilder sb = new StringBuilder();
            foreach (byte b in buffer)
                sb.Append(b.ToString("X2"));
            return sb.ToString();
        }
        public static Size GetImageSize(this Stream stream)
        {
            Image image = Image.FromStream(stream);
            stream.Position = 0;
            return image.Size;
        }
        public static Size GetImageSize(this byte[] bytes)
        {
            using (Stream stream = new MemoryStream(bytes))
            {
                Image image = Image.FromStream(stream);
                return image.Size;
            }
        }
    }
}
