﻿using JYXWeb.DB;
using JYXWeb.Models;
using JYXWeb.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JYXWeb.Controllers
{
    [Authorize]
    public class PackageController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.angularAppName = "packageApp";
            ViewBag.angularControllerName = "packageCtrl as vm";
            return View();
        }

        public ActionResult New()
        {
            ViewBag.angularAppName = "packageApp";
            ViewBag.angularControllerName = "packageCtrl as vm";
            return View();
        }

        public ActionResult GetPackageOverview()
        {
            using (var dataContext = new PackageDataContext())
            {
                var overview = dataContext.Packages.Where(a => a.UserCode == User.Identity.GetUserCode()).GroupBy(a => a.Status).Select(a => new
                {
                    Status = a.Key,
                    Count = a.Count(),
                }).ToList();
                return Json(overview, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SearchPackages(string criteriaStr)
        {
            dynamic criteria = JsonConvert.DeserializeObject(criteriaStr);
            DateTime startDate = criteria.startDate;
            DateTime endDate = criteria.endDate;
            string status = criteria.status;
            string receiver = criteria.receiver;
            string tracking = criteria.tracking;
            string packageCode = criteria.packageCode;
            string userName = criteria.userName;
            string userCode = criteria.userCode;
            userCode = userCode == null ? "" : userCode.ToUpper();
            userName = userName == null ? "" : userName.ToLower();
            var isAdmin = AppUtil.IsAdmin(User.Identity.GetUserCode());
            if (!isAdmin && userCode != User.Identity.GetUserCode() ||
                !isAdmin && userCode == "")
            {
                return null;
            }

            var userCodes = new string[] { userCode };
            if (userCode == "")
            {
                var userNames = new ApplicationDbContext().Users.Select(a => new
                {
                    Name = (a.FirstName == null ? "" : a.FirstName.ToLower()) + " " + (a.LastName == null ? "" : a.LastName.ToLower()),
                    UserCode = a.UserCode,
                }).ToArray();
                userCodes = userNames.Where(a => userName == null || userName == "" || a.Name.Contains(userName)).Select(a => a.UserCode).ToArray();
            }

            using (var dataContext = new PackageDataContext())
            {
                var jointPackages = userCodes.Join(dataContext.Packages, a => a, b => b.UserCode, (a, b) => b);
                var packages = jointPackages.Where(a =>
                    a.LastUpdateTime >= startDate && a.LastUpdateTime <= endDate &&
                    (packageCode == "" || packageCode == a.ID) &&
                    (status == null || status == "全部" || (!isAdmin && status == a.Status || isAdmin && status == a.SubStatus)) &&
                    (receiver == "" || receiver == a.Address.Name) &&
                    (tracking == "" || a.Products.Where(b => b.Tracking == tracking).Count() > 0)).Select(a => new
                    {
                        a.ID,
                        Tracking = String.Join(";", a.Products.Select(b => b.Tracking).Distinct()),
                        Receiver = a.Address == null ? "" : a.Address.Name,
                        AddressID = a.AddressID == null ? "" : a.AddressID.ToString(),
                        ProductNames = String.Join(", ", a.Products.Select(b => b.Name + " * " + b.Quantity)),
                        Status = a.Status,
                        SubStatus = a.SubStatus,
                        a.UserCode,
                        Disabled = a.Status != "已入库" && a.Status != "待入库",
                    }).ToList();
                return Json(packages);
            }
        }

        public ActionResult GetPackage(string id)
        {
            using (var packageDataConext = new PackageDataContext())
            {
                var package = packageDataConext.Packages.Where(a => a.ID == id).SingleOrDefault();
                if (package != null)
                {
                    var newObj = new
                    {
                        package.ID,
                        OrderNumber = string.Join(";", package.Products.Select(a => a.OrderNumber).Distinct()),
                        Tracking = string.Join(";", package.Products.Select(a => a.Tracking).Distinct()),
                        Notes = string.Join(";", package.Products.Select(a => a.Notes).Distinct()),
                        package.Status,
                        package.SubStatus,
                        Sender = package.SenderID == null ? null :new {
                            package.Sender.ID,
                            package.Sender.Name,
                            package.Sender.Address,
                            package.Sender.Phone,
                        },
                        Address = package.AddressID == null ? null : new
                        {
                            package.Address.ID,
                            ProvinceName = package.Address.District1.District1.District1.Name,
                            Province = package.Address.District1.District1.District1.ID,
                            CityName = package.Address.District1.District1.Name,
                            City = package.Address.District1.District1.ID,
                            DistrictName = package.Address.District1.Name,
                            package.Address.District,
                            package.Address.Street,
                            package.Address.Name,
                            package.Address.Phone,
                            package.Address.PostCode,
                            package.Address.IsDefault,
                            package.Address.UserCode,
                        },
                        Products = package.Products.Select((a, index) => new
                        {
                            a.Name,
                            a.Brand,
                            a.Price,
                            a.Quantity,
                            a.ID,
                            a.Tracking,
                            Number = index + 1,
                            a.Category,
                        }),
                    };
                    return Json(newObj, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        public ActionResult UpdatePackage(Package package)
        {
            using (var packageDataConext = new PackageDataContext())
            {
                var existingPackage = packageDataConext.Packages.Where(a => a.ID == package.ID).SingleOrDefault();
                if (existingPackage != null)
                {
                    existingPackage.AddressID = package.AddressID;
                    existingPackage.SenderID = package.SenderID;
                    existingPackage.Status = package.Status;
                    existingPackage.SubStatus = package.SubStatus;
                    packageDataConext.Products.DeleteAllOnSubmit(existingPackage.Products);
                    existingPackage.Products.Clear();
                    existingPackage.Products.AddRange(package.Products);
                    existingPackage.LastUpdateTime = DateTime.Now;
                }
                else
                {
                    package.ID = GeneratePackageCode();
                    package.UserCode = User.Identity.GetUserCode();
                    package.Sender = packageDataConext.Senders.Where(a => a.ID == package.Sender.ID).SingleOrDefault();
                    package.Address = packageDataConext.Addresses.Where(a => a.ID == package.Address.ID).SingleOrDefault();
                    package.LastUpdateTime = DateTime.Now;
                    packageDataConext.Packages.InsertOnSubmit(package);
                }
                packageDataConext.SubmitChanges();
            }
            return null;
        }

        public ActionResult DeletePackage(string id)
        {
            using (var packageDataConext = new PackageDataContext())
            {
                packageDataConext.Packages.DeleteAllOnSubmit(packageDataConext.Packages.Where(a => a.ID == id));
                packageDataConext.SubmitChanges();
            }
            return null;
        }

        public ActionResult CombinePackages(string[] packageCodes)
        {
            using (var packageDataConext = new PackageDataContext())
            {
                var packages = packageCodes.Join(packageDataConext.Packages, a => a, b => b.ID, (a, b) => b).ToList();
                var newPackage = new Package();
                newPackage.AddressID = packages[0].AddressID;
                newPackage.Products.AddRange(packages.SelectMany(a=>a.Products));
                newPackage.Status = "待入库";
                newPackage.LastUpdateTime = DateTime.Now;
                newPackage.ID = GeneratePackageCode();
                newPackage.UserCode = User.Identity.GetUserCode();
                packageDataConext.Packages.InsertOnSubmit(newPackage);
                packageDataConext.Packages.DeleteAllOnSubmit(packages);
                packageDataConext.SubmitChanges();
            }
            return null;
        }

        public ActionResult GetProductCategories()
        {
            using (var packageDataConext = new PackageDataContext())
            {
                var result = packageDataConext.ProductCategories.GroupBy(a => a.Port).Select(a => new
                {
                    Port = a.Key,
                    Categories = a.Select(b => new
                    {
                        b.ID,
                        b.Name,
                        b.Category
                    })
                }).ToArray();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Tracking(string id)
        {
            return Json(GetTrackingInfo(id), JsonRequestBehavior.AllowGet);
        }

        public IList<string[]> GetTrackingInfo(string id)
        {
            var trackingInfo = new List<string[]>();
            var trackingRawHtml = AppUtil.SubmitUrl(TRACKING_URL + id);
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(trackingRawHtml);

            var className = "commnTxt";
            var nodes = doc.DocumentNode.Descendants("div")
                .Where(d => d.Attributes.Contains("class") && d.Attributes["class"].Value.Contains(className));
            foreach (var node in nodes)
            {
                var trs = node.Descendants("tr").ToList();
                for (var i = 1; i < trs.Count; i++)
                {
                    var tds = trs[i].Descendants("td").ToList();
                    trackingInfo.Add(new string[] { tds[1].InnerText, tds[2].InnerText });
                }
            }
            return trackingInfo;
        }

        public string GeneratePackageCode()
        {
            var code = "";
            using (var packageDataContext = new PackageDataContext())
            {
                var maxCode = packageDataContext.Packages.Select(a => a.ID).Max(a => a);
                code = "TM" + (long.Parse(maxCode.Replace("TM", "")) + 1);
            }
            return code;
        }

        public string CreateTMEntry()
        {
            var tracking = new TMTracking();
            return TMUtil.CreateTMEntry(tracking);
        }

        public const string TRACKING_URL = "http://www.expresstochina.com/TrackSearch.aspx?TXT_TRACKNO=";

        public const string PACKAGE_STATUS_NO_NOTICE = "未预报";
        public const string PACKAGE_STATUS_AWAIT = "待入库";
        public const string PACKAGE_STATUS_IN_WAREHOUSE = "已入库";
        public const string PACKAGE_STATUS_OUT_OF_WAREHOUSE = "已出库";
        public const string PACKAGE_STATUS_ON_PLAIN = "空运";
        public const string PACKAGE_STATUS_CUSTOMS_CLEARANCE = "清关中";
        public const string PACKAGE_STATUS_DOMESTIC_TRANSPORTATION = "国内转运";

    }
}