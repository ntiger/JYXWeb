using JYXWeb.DB;
using JYXWeb.Models;
using JYXWeb.Util;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;

namespace JYXWeb.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.angularAppName = "transactionApp";
            ViewBag.angularControllerName = "transactionCtrl";
            return View();
        }

    }
}