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
                var inputBytes = Encoding.ASCII.GetBytes(input);
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
    }

}