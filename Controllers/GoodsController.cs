using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JYXWeb.Controllers
{
    public class GoodsController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.angularAppName = "goodsApp";
            ViewBag.angularControllerName = "goodsCtrl";
            return View();
        }


        public ActionResult Upload()
        {
            ViewBag.angularAppName = "goodsApp";
            ViewBag.angularControllerName = "goodsCtrl";
            return View();
        }
    }
}