using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace JYXWeb.Util
{
    public class AppUtil
    {
        public static string GetUserCode(int size = 6)
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