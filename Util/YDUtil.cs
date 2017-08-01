using JYXWeb.DB;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Web;
using System.Security.Cryptography;

namespace JYXWeb.Util
{
    public class YDUtil
    {
        #region Constants

        public const string LINK_CREATE_PACKAGE = "http://120.55.73.189:9955/formjson/parcel_create.html";

        public const string KEY = "xiaoyangAPi";
        public const string PASS = "fOFCxjKnEtCrw4TPB7v0kATE5b3Iiy9j";

        public const string TRACKING_URL = "http://www.takeex.com/track.html";
        public const string TRACKING_HEADER_REFERER = "http://www.takeex.com/";

        #endregion

        public string YDRequest(string jsonString)
        {
            var client = new RestClient(LINK_CREATE_PACKAGE);
            var _cookieJar = new CookieContainer();
            client.CookieContainer = _cookieJar;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddHeader("key", KEY);
            request.AddHeader("sign", CreateMD5(jsonString + PASS));
            var paramStr = "";
            paramStr += "json=" + jsonString;
            paramStr += "&version=1.0";
            request.AddParameter("application/x-www-form-urlencoded", paramStr, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            return response.Content;
        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        public static IList<string[]> GetTrackingInfo(string id)
        {
            var trackingInfo = new List<string[]>();
            var paramsDict = new Dictionary<string, string>() { { "data", id }, };
            var trackingRawHtmlBytes = AppUtil.PostUrl(TRACKING_URL, paramsDict,
                new Dictionary<string, string> { { "Referer", TRACKING_HEADER_REFERER } });
            var responseJsonString = Encoding.UTF8.GetString(trackingRawHtmlBytes);
            dynamic trackingJson = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<object>(responseJsonString);
            foreach (var data in trackingJson["data"][0])
            {
                trackingInfo.Insert(trackingInfo.Count > 0 ? 1 : 0, new string[] { data["date"].Replace("速佳快递", "美天优递"), data["info"].Replace("速佳", "").Replace("韵达", "") });
            }

            return trackingInfo;
        }
    }

}