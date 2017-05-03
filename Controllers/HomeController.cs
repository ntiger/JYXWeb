using JYXWeb.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace JYXWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.angularAppName = "homeApp";
            ViewBag.angularControllerName = "homeCtrl";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Service()
        {
            return View();
        }
        

        #region News

        public ActionResult News()
        {
            return View();
        }

        public ActionResult GetNews()
        {

            return Json(null);
        }

        public ActionResult GetNewsEntry()
        {
            return Json(null);
        }

        public ActionResult CreateNews()
        {
            return null;
        }

        public ActionResult PostNews()
        {
            return null;
        }

        #endregion

        #region Test

        public ActionResult Test()
        {
            ViewBag.angularAppName = "testApp";
            ViewBag.angularControllerName = "testCtrl";
            return View();
        }

        public ActionResult Test1()
        {
            ViewBag.angularAppName = "testApp";
            ViewBag.angularControllerName = "testCtrl";
            return View();
        }

        public ActionResult TestYD()
        {
            var package = new
            {
                trknum = "TEST12345678",
                outerTrknum = "OUT87654321",
                senderName = "张发",
                senderPhone = "812345678",

                receiverName = "李收",
                receiverPhone = "712345678",
                receiverCountry = "CN",
                receiverProvince = "辽宁",
                receiverCity = "大连",
                receiverDistrict = "高新区",
                receiverAddress = "火炬路1号",

                weightLb = "2.3",
                declaredValueUsd = "33.0",
                insuredValueUsd = "10.3",
                items = new int[1].Select(a => new
                {
                    name = "手提包",
                    brand = "MK",
                    quantity = 2,
                    spec = "",
                    description = "长的描述",
                }),
            };


            var serializer = new JavaScriptSerializer();
            var jsonString = serializer.Serialize(package);

            var ydUtil = new YDUtil();
            return Json(ydUtil.YDRequest(jsonString), JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}