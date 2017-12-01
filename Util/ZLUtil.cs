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
using NPOI.HSSF.UserModel;
using System.IO;

namespace JYXWeb.Util
{
    public class ZLUtil
    {
        #region Constants

        public const string TRACKING_URL = "http://tstexp.com/index.php?c=search&f=show";

        #endregion

        public static IList<string[]> GetTrackingInfo(string id)
        {
            var package = new PackageDataContext().Packages.Where(a => a.ID == id).FirstOrDefault();

            var trackingInfo = new List<string[]>();
            var trackingRawHtmlBytes = AppUtil.PostUrl(TRACKING_URL, new Dictionary<string, string>() { { "keywords", package.IDOther } });
            var trackingRawHtml = Encoding.UTF8.GetString(trackingRawHtmlBytes);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(trackingRawHtml);

            var className = "wapbox";
            var divNode = doc.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains(className)).LastOrDefault();
            var nodes = divNode.Descendants("dl").ToList();
            for (var i = 0; i < nodes.Count; i++)
            {
                if (trackingInfo.Count>0 && trackingInfo.Last()[1].IndexOf("快递单号:") > -1)
                {
                    trackingInfo.Add(new string[] { nodes[i].Descendants("dt").First().InnerText, nodes[i].Descendants("dd").First().InnerText });
                }
                if (trackingInfo.Count > 0 && trackingInfo.Last()[1] == "航班抵达 转交海关清关中")
                {
                    trackingInfo.Add(new string[] { nodes[i].Descendants("dt").First().InnerText, nodes[i].Descendants("dd").First().InnerText });
                }
            }
            if (id == "ZTM037390043101" || id == "ZTM037394458401")
            {
                trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddHours(-4).ToString("yyyy-MM-dd HH:mm") : "", "运单创建, 等待处理" });
                trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddDays(3).ToString("yyyy-MM-dd HH:mm") : "", "已出库, 送往集散中心" });
                if (DateTime.Now > package.CreateTime.Value.AddDays(6).AddMinutes(51))
                {
                    trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddDays(6).AddMinutes(51).ToString("yyyy-MM-dd HH:mm") : "", "集散中心拦收" });
                }
                if (DateTime.Now > package.CreateTime.Value.AddDays(8).AddMinutes(12))
                {
                    trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddDays(8).AddMinutes(12).ToString("yyyy-MM-dd HH:mm") : "", "送机场，等待飞行" });
                }
                if (DateTime.Now > package.CreateTime.Value.AddDays(10).AddMinutes(33))
                {
                    trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddDays(10).AddMinutes(33).ToString("yyyy-MM-dd HH:mm") : "", "飞机起飞，飞行中" });
                }
                if (DateTime.Now > package.CreateTime.Value.AddDays(12).AddMinutes(122))
                {
                    trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddDays(12).AddMinutes(122).ToString("yyyy-MM-dd HH:mm") : "", "清关中" });
                }
            }
            else
            {
                trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddHours(-4).ToString("yyyy-MM-dd HH:mm") : "", "运单创建, 等待处理" });
                trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.ToString("yyyy-MM-dd HH:mm") : "", "已出库, 送往集散中心" });
                if (DateTime.Now > package.CreateTime.Value.AddDays(2).AddMinutes(51))
                {
                    trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddDays(2).AddMinutes(51).ToString("yyyy-MM-dd HH:mm") : "", "集散中心拦收" });
                }
                if (DateTime.Now > package.CreateTime.Value.AddDays(4).AddMinutes(12))
                {
                    trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddDays(4).AddMinutes(12).ToString("yyyy-MM-dd HH:mm") : "", "送机场，等待飞行" });
                }
                if (DateTime.Now > package.CreateTime.Value.AddDays(6).AddMinutes(33))
                {
                    trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddDays(6).AddMinutes(33).ToString("yyyy-MM-dd HH:mm") : "", "飞机起飞，飞行中" });
                }
                if (DateTime.Now > package.CreateTime.Value.AddDays(8).AddMinutes(122))
                {
                    trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddDays(8).AddMinutes(122).ToString("yyyy-MM-dd HH:mm") : "", "清关中" });
                }
            }

            return trackingInfo.OrderBy(a => a[0]).ToList();
        }

    }

}