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
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace JYXWeb.Util
{
    public class HMUtil
    {
        #region Constants

        public const string TRACKING_URL = "http://www.hmus.net/tracking.aspx?yundanno=";
        public const string TRACKING_URL_TAIL = "&yd=j0ZnDdBJlSg=";

        public const string USERNAME = "yunyunwd@gmail.com";
        public const string PASSWORD = "yun129715";

        #endregion


        public static IList<string[]> GetTrackingInfo(string id)
        {
            var trackingInfo = new List<string[]>();
            var trackingRawHtml = AppUtil.SubmitUrl(TRACKING_URL + id + TRACKING_URL_TAIL, null);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(trackingRawHtml);

            var className = "track-text";
            var nodes = doc.DocumentNode.Descendants("td").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains(className)).ToList();
            for (var i = 0; i < nodes.Count; i++)
            {
                if (i % 2 == 1)
                {
                    trackingInfo.Add(new string[] { nodes[i - 1].InnerText, nodes[i].InnerText });
                }
            }
            return trackingInfo;
        }

    }

}