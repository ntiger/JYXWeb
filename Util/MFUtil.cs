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
using JYXWeb.Controllers;

namespace JYXWeb.Util
{
    public class MFUtil
    {
        #region Constants

        public const string TRACKING_URL = "http://www.bee001.net/home/SearchTrack?codeString=";

        public const string USERNAME = "yangjilai@gmail.com";
        public const string PASSWORD = "mifeng123";

        #endregion


        public static IList<string[]> GetTrackingInfo(string id, string packageID)
        {
            var trackingInfo = new List<string[]>();
            var trackingRawHtml = AppUtil.SubmitUrl(TRACKING_URL + id, null);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(trackingRawHtml);

            var className = "bodys7-left-list-b";
            var nodes = doc.DocumentNode.Descendants("div").Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains(className)).ToList();
            var package = new PackageDataContext().Packages.Where(a => a.ID == packageID).FirstOrDefault();
            if (package.Status == PackageController.PACKAGE_STATUS_AWAIT)
            {
                trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.ToString("yyyy-MM-dd HH:mm") : "", "运单创建, 等待处理" });
            }
            if(package.Status == PackageController.PACKAGE_STATUS_OUT_OF_WAREHOUSE)
            {
                trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.AddHours(-4).ToString("yyyy-MM-dd HH:mm") : "", "运单创建, 等待处理" });
                trackingInfo.Add(new string[] { package.CreateTime.HasValue ? package.CreateTime.Value.ToString("yyyy-MM-dd HH:mm") : "", "已出库, 送往集散中心" });
            }
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].ChildNodes["p"].InnerText.Contains("东部仓到签"))
                {
                    trackingInfo.Add(new string[] { nodes[i].ChildNodes["span"].InnerText, "集散中心揽收" });
                }
                if (nodes[i].ChildNodes["p"].InnerText.Contains("航班已起飞"))
                {
                    trackingInfo.Add(new string[] { nodes[i].ChildNodes["span"].InnerText, "航班已起飞" });
                }
                if (nodes[i].ChildNodes["dd"].InnerText.Contains("清关中"))
                {
                    trackingInfo.Add(new string[] { nodes[i].ChildNodes["span"].InnerText, "清关中" });
                }
                if (nodes[i].ChildNodes["p"].InnerText.Contains("清关已完成"))
                {
                    trackingInfo.Add(new string[] { nodes[i].ChildNodes["span"].InnerText, "清关已完成" });
                }
                if (nodes[i].ChildNodes["p"].InnerText.Contains("口岸转运确认"))
                {
                    trackingInfo.Add(new string[] { nodes[i].ChildNodes["span"].InnerText, nodes[i].ChildNodes["dd"].InnerText.Replace("快递机构", "已转运") });
                }
            }
            return trackingInfo;
        }

        public static byte[] ExportXLSOpenXML(Package[] packages)
        {
            // Creating the workbook...
            var templateWorkbook = new XSSFWorkbook();

            // Getting the worksheet by its name...
            var sheet = templateWorkbook.GetSheet("Sheet1");
            if (sheet == null)
            {
                sheet = templateWorkbook.CreateSheet("Sheet1");
            }

            var row = 0;
            var col = 0;

            var dataRow = sheet.CreateRow(row);
            dataRow.CreateCell(col++).SetCellValue("序号");
            dataRow.CreateCell(col++).SetCellValue("订单号");
            dataRow.CreateCell(col++).SetCellValue("收件人");
            dataRow.CreateCell(col++).SetCellValue("收件公司");
            dataRow.CreateCell(col++).SetCellValue("收件人电话");
            dataRow.CreateCell(col++).SetCellValue("收件国家");
            dataRow.CreateCell(col++).SetCellValue("收件省");
            dataRow.CreateCell(col++).SetCellValue("收件市");
            dataRow.CreateCell(col++).SetCellValue("收件区县");
            dataRow.CreateCell(col++).SetCellValue("收件详细地址");
            dataRow.CreateCell(col++).SetCellValue("收件邮编");
            dataRow.CreateCell(col++).SetCellValue("身份证号");
            dataRow.CreateCell(col++).SetCellValue("备注");
            dataRow.CreateCell(col++).SetCellValue("是否保价");
            dataRow.CreateCell(col++).SetCellValue("品名");
            dataRow.CreateCell(col++).SetCellValue("品牌");
            dataRow.CreateCell(col++).SetCellValue("数量");
            dataRow.CreateCell(col++).SetCellValue("价值");
            dataRow.CreateCell(col++).SetCellValue("SKU");
            dataRow.CreateCell(col++).SetCellValue("货架号");
            dataRow.CreateCell(col++).SetCellValue("UPC");

            row++;
            for (var i = 0; i < packages.Length; i++)
            {
                col = 0;
                var package = packages[i];
                dataRow = sheet.CreateRow(row);
                dataRow.CreateCell(col++).SetCellValue(i + 1);
                dataRow.CreateCell(col++).SetCellValue(package.ID);
                dataRow.CreateCell(col++).SetCellValue(package.Address.Name);
                dataRow.CreateCell(col++).SetCellValue("");
                dataRow.CreateCell(col++).SetCellValue(package.Address.Phone);
                dataRow.CreateCell(col++).SetCellValue("中国大陆");
                dataRow.CreateCell(col++).SetCellValue(package.Address.District1.District1.District1.Name);
                dataRow.CreateCell(col++).SetCellValue(package.Address.District1.District1.Name);
                dataRow.CreateCell(col++).SetCellValue(package.Address.District1.Name);
                dataRow.CreateCell(col++).SetCellValue(package.Address.Street);
                dataRow.CreateCell(col++).SetCellValue("111111");
                dataRow.CreateCell(col++).SetCellValue(package.Address.IDCard);
                dataRow.CreateCell(col++).SetCellValue("");
                dataRow.CreateCell(col++).SetCellValue("否");

                dataRow.CreateCell(col++).SetCellValue(package.Products[0].Name);
                dataRow.CreateCell(col++).SetCellValue(package.Products[0].Brand);
                dataRow.CreateCell(col++).SetCellValue(package.Products[0].Quantity == null ? "" : package.Products[0].Quantity.ToString());
                dataRow.CreateCell(col++).SetCellValue(package.Products[0].Price == null ? "" : package.Products[0].Price.ToString());
                dataRow.CreateCell(col++).SetCellValue("");
                dataRow.CreateCell(col++).SetCellValue(package.Weight ?? package.WeightEst ?? 2);
                dataRow.CreateCell(col++).SetCellValue("");

                foreach (var product in package.Products.Skip(1))
                {
                    row++;
                    col = 14;
                    dataRow = sheet.CreateRow(row);
                    dataRow.CreateCell(col++).SetCellValue(product.Name);
                    dataRow.CreateCell(col++).SetCellValue(product.Brand);
                    dataRow.CreateCell(col++).SetCellValue(product.Quantity == null ? "" : product.Quantity.ToString());
                    dataRow.CreateCell(col++).SetCellValue(product.Price == null ? "" : product.Price.ToString());
                }
                row++;
            }

            MemoryStream ms = new MemoryStream();

            // Writing the workbook content to the FileStream...
            templateWorkbook.Write(ms);

            // Sending the server processed data back to the user computer...
            return ms.ToArray();
        }
        
    }

}