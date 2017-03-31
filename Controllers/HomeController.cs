﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
    }
}