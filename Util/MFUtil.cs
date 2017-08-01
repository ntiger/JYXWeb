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
    public class MFUtil
    {
        #region Constants

        public const string TRACKING_URL = "http://www.bee001.net/home/SearchTrack?codeString=";

        public const string USERNAME = "yangjilai@gmail.com";
        public const string PASSWORD = "mifeng123";

        #endregion


        public static IList<string[]> GetTrackingInfo(string id)
        {
            var trackingInfo = new List<string[]>();
            var trackingRawHtml = AppUtil.SubmitUrl(TRACKING_URL + id, null);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(trackingRawHtml);

            var className = "bodys7-left-list-b";
            var nodes = doc.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains(className)).ToList();
            for (var i = 0; i < nodes.Count; i++)
            {
                trackingInfo.Add(new string[] { nodes[i].ChildNodes["span"].InnerText, nodes[i].ChildNodes["p"].InnerText });
            }
            return trackingInfo;
        }

    }

}