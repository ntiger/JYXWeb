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
        public const string PASS = "fOFCxjKnEtCrw4TPB7v0kATE5b3liy9j";
        
        #endregion

        public void YDRequest(string jsonString)
        {
            var client = new RestClient();
            var _cookieJar = new CookieContainer();
            client.CookieContainer = _cookieJar;
            var request = new RestRequest(Method.POST);
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("key", KEY);
            request.AddHeader("sign", GetMD5Format(jsonString + PASS));
            var paramStr = "";
            request.AddParameter("application/x-www-form-urlencoded", paramStr, ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

        }

        public string GetMD5Format(string input)
        {
            using (var md5 = MD5.Create(input))
            {
                return Encoding.UTF8.GetString(md5.Hash);
            }
        }
    }

}