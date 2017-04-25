using System;
using System.Collections.Generic;
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
        public static string GenerateUserCode(int size = 6)
        {
            string input = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Random random = new Random();
            var chars = Enumerable.Range(0, size)
                                   .Select(x => input[random.Next(0, input.Length)]);
            return new string(chars.ToArray());
        }

        public static bool IsAdmin(string userCode)
        {
            if (userCode == "AAAAAA" || userCode == "BBBBBB")
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
            var barcode = new Code128BarcodeDraw(checksum).Draw(id, 80, 3);
            return barcode;
        }


        #region External Request

        public static void SubmitUrlAsync(string url)
        {
            Task.Factory.StartNew(() => SubmitUrlPrivate(url));
        }

        public static string SubmitUrl(string url)
        {
            return SubmitUrlPrivate(url);
        }

        private static string SubmitUrlPrivate(string url)
        {
            string responsebody;
            using (var webClient = new WebClient())
            {
                try
                {
                    using (var stream = webClient.OpenRead(url))
                    {
                        responsebody = new StreamReader(stream).ReadToEnd();
                    }
                }
                catch (Exception)
                {
                    responsebody = "Error";
                }
            }

            return responsebody;
        }


        public static void PostUrlAsync(string url, IDictionary<string, string> keyValuePairs)
        {
            Task.Factory.StartNew(() => PostUrlPrivate(url, keyValuePairs));
        }

        public static string PostUrl(string url, IDictionary<string, string> keyValuePairs)
        {
            return PostUrlPrivate(url, keyValuePairs);
        }

        private static string PostUrlPrivate(string url, IDictionary<string, string> keyValuePairs)
        {
            string responsebody;
            using (var webClient = new WebClient())
            {
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
                    responsebody = Encoding.UTF8.GetString(responsebytes);
                }
                catch (Exception)
                {
                    responsebody = "Error";
                }
            }
            return responsebody;
        }

        

        #endregion
    }
}