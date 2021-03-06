﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Zen.Barcode;

namespace JYXWeb.Util
{
    public class AppUtil
    {
        public static string GenerateUserCode(int size = 5)
        {
            string input = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Random random = new Random();
            var chars = Enumerable.Range(0, size)
                                   .Select(x => input[random.Next(0, input.Length)]);
            return new string(chars.ToArray());
        }

        public static bool IsAdmin(string userCode)
        {
            if (userCode == "AAAAA" || userCode == "BBBBB")
            {
                return true;
            }
            return false;
        }

        public static string GetContentType(string fileName)
        {
            string contentType = "application/octetstream";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (registryKey != null && registryKey.GetValue("Content Type") != null)
                contentType = registryKey.GetValue("Content Type").ToString();
            return contentType;
        }

        #region Image Handling

        public static string GetThumbnail(string sourceImageBase64, int? outputWidth, bool keepOriginalRatio = true)
        {
            if (sourceImageBase64 == "" || outputWidth == null) { return sourceImageBase64; }
            var imageBytes = Convert.FromBase64String(sourceImageBase64);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image sourceImage = Image.FromStream(ms, true);
            var width = outputWidth.Value;
            if (width >= sourceImage.Width) { return sourceImageBase64; }
            int X = sourceImage.Width;
            int Y = sourceImage.Height;
            int height = (int)((width * Y) / X);
            if (!keepOriginalRatio)
            {
                height = width * 618 / 1000;
            }
            string base64String;
            using (var thumbnail = sourceImage.GetThumbnailImage(width, height, new Image.GetThumbnailImageAbort(ThumbnailCallback), IntPtr.Zero))
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    thumbnail.Save(memoryStream, sourceImage.RawFormat);
                    Byte[] bytes = new Byte[memoryStream.Length];
                    memoryStream.Position = 0;
                    memoryStream.Read(bytes, 0, (int)bytes.Length);
                    base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
                }
            }
            return base64String;
        }

        private static bool ThumbnailCallback()
        {
            return false;
        }

        public static string ResizeImage(string sourceImageBase64, int? outputWidth, bool keepOriginalRatio = true)
        {
            if (sourceImageBase64 == "" || outputWidth == null) { return sourceImageBase64; }
            var imageBytes = Convert.FromBase64String(sourceImageBase64);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            Image sourceImage = Image.FromStream(ms, true);
            var width = outputWidth.Value;
            if (width >= sourceImage.Width) { return sourceImageBase64; }
            int X = sourceImage.Width;
            int Y = sourceImage.Height;
            int height = (int)((width * Y) / X);
            if (!keepOriginalRatio)
            {
                height = width * 618 / 1000;
            }

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(sourceImage, destRect, 0, 0, sourceImage.Width, sourceImage.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            string base64String;
            using (MemoryStream memoryStream = new MemoryStream())
            {
                destImage.Save(memoryStream, sourceImage.RawFormat);
                Byte[] bytes = new Byte[memoryStream.Length];
                memoryStream.Position = 0;
                memoryStream.Read(bytes, 0, (int)bytes.Length);
                base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
            }

            return base64String;
        }

        public static Image GetBarcodeImage(string id)
        {
            var checksum = Code128Checksum.Instance;
            var barcode = new Code128BarcodeDraw(checksum).Draw(id, 30, 1);
            return barcode;
        }

        #endregion

        #region External Request

        public static void SubmitUrlAsync(string url, Dictionary<string, string> headerParams = null)
        {
            Task.Factory.StartNew(() => SubmitUrlPrivate(url, headerParams));
        }

        public static string SubmitUrl(string url, Dictionary<string, string> headerParams = null)
        {
            return SubmitUrlPrivate(url, headerParams);
        }

        private static string SubmitUrlPrivate(string url, Dictionary<string, string> headerParams)
        {
            string responsebody;
            using (var webClient = new WebClient())
            {
                if (headerParams != null)
                {
                    foreach (var keyValue in headerParams)
                    {
                        webClient.Headers.Add(keyValue.Key, keyValue.Value);
                    }
                }
                try
                {
                    using (var stream = webClient.OpenRead(url))
                    {
                        if (webClient.ResponseHeaders["Content-Type"] != null && webClient.ResponseHeaders["Content-Type"].ToUpper().Contains("GB2312"))
                        {
                            responsebody = new StreamReader(stream, Encoding.GetEncoding("GB2312")).ReadToEnd();
                        }
                        else
                        {
                            responsebody = new StreamReader(stream).ReadToEnd();
                        }
                    }
                }
                catch (Exception)
                {
                    responsebody = "Error";
                }
            }

            return responsebody;
        }


        public static void PostUrlAsync(string url, IDictionary<string, string> keyValuePairs, Dictionary<string, string> headerParams = null)
        {
            Task.Factory.StartNew(() => PostUrlPrivate(url, keyValuePairs, headerParams));
        }

        public static byte[] PostUrl(string url, IDictionary<string, string> keyValuePairs, Dictionary<string, string> headerParams = null)
        {
            return PostUrlPrivate(url, keyValuePairs, headerParams);
        }

        private static byte[] PostUrlPrivate(string url, IDictionary<string, string> keyValuePairs, Dictionary<string, string> headerParams = null)
        {
            byte[] responsebody;
            using (var webClient = new WebClient())
            {
                if (headerParams != null)
                {
                    foreach (var keyValue in headerParams)
                    {
                        webClient.Headers.Add(keyValue.Key, keyValue.Value);
                    }
                }
                try
                {
                    System.Collections.Specialized.NameValueCollection reqparm = new System.Collections.Specialized.NameValueCollection();
                    if (keyValuePairs != null)
                    {
                        foreach (var entry in keyValuePairs)
                        {
                            reqparm.Add(entry.Key, entry.Value);
                        }
                    }
                    byte[] responsebytes = webClient.UploadValues(url, "POST", reqparm);
                    responsebody = responsebytes;
                }
                catch (Exception e)
                {
                    responsebody = Encoding.UTF8.GetBytes("Error");
                    Console.WriteLine(e.Message);
                }
            }
            return responsebody;
        }



        #endregion


        #region File Handling

        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            var dt = new DataTable();
            using (var sr = new StreamReader(strFilePath))
            {
                var headers = sr.ReadLine().Split(',');
                foreach (var header in headers)
                {
                    dt.Columns.Add(header);
                }

                while (!sr.EndOfStream)
                {
                    var rows = sr.ReadLine().Split(',');
                    if (rows.Length > 1)
                    {
                        var dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i].Trim();
                        }
                        dt.Rows.Add(dr);
                    }
                }

            }


            return dt;
        }

        public static DataTable ConvertXSLXtoDataTable(string strFilePath, string connString)
        {
            var oledbConn = new OleDbConnection(connString);
            var dt = new DataTable();
            try
            {
                oledbConn.Open();
                var dbSchema = oledbConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                var sheet1 = dbSchema.Rows[0].Field<string>("TABLE_NAME");
                using (var cmd = new OleDbCommand("SELECT * FROM ["+ sheet1 + "]", oledbConn))
                {
                    var oleda = new OleDbDataAdapter();
                    oleda.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    oleda.Fill(ds);
                    dt = ds.Tables[0];
                }
            }
            catch(Exception e)
            {
                Console.Write(e.Message);
            }
            finally
            {
                oledbConn.Close();
            }
            return dt;
        }

        #endregion
    }
}