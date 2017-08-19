using JYXWeb.DB;
using JYXWeb.Models;
using JYXWeb.Util;
using Newtonsoft.Json;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using System;
using System.Linq;
using System.Web.Mvc;

namespace JYXWeb.Controllers
{
    [Authorize]
    public class PackageController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.angularAppName = "packageApp";
            ViewBag.angularControllerName = "packageCtrl";
            return View();
        }

        public ActionResult New()
        {
            ViewBag.angularAppName = "packageApp";
            ViewBag.angularControllerName = "packageCtrl";
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
                    (receiver == "" || a.Address!=null && receiver == a.Address.Name) &&
                    (tracking == "" || a.Products.Where(b => b.Tracking == tracking).Count() > 0)).Select(a => new
                    {
                        a.ID,
                        Tracking = String.Join("; ", a.Products.Select(b => b.Tracking).Distinct()),
                        Receiver = a.Address == null ? "" : a.Address.Name,
                        AddressID = a.AddressID == null ? "" : a.AddressID.ToString(),
                        ProductNames = String.Join(", ", a.Products.Select(b => b.Name + " * " + b.Quantity)),
                        Status = a.Status,
                        SubStatus = a.SubStatus,
                        a.UserCode,
                        Weight = a.Weight == null ? a.WeightEst + " (预估)" : a.Weight + "",
                        Cost = string.Format("{0:c}", a.Cost != null ? a.Cost.Value :
                                RoundPackageWeight(a.Weight ?? a.WeightEst ?? 2) *
                                (a.Products.Count == 0 || a.Products.Where(b => b.Channel != null).Count() == 0 ? null :
                                a.Products.Where(b => b.Channel == a.Products.Max(c => c.Channel)).First()
                                .Channel1.Pricings.Where(c => c.UserCode == a.UserCode).Select(c => c.Price).SingleOrDefault() ??
                                a.Products.Where(b => b.Channel == a.Products.Max(c => c.Channel)).First().Channel1.DefaultPrice)) +
                                (a.Weight == null ? " (预估)" : ""),
                        Disabled = a.SubStatus != "已入库" && a.SubStatus != "待入库",
                        a.LastUpdateTime,
                    }).OrderByDescending(a => a.LastUpdateTime).ToList();
                var jsonResult = Json(packages);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
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
                        OriginalID = package.ID,
                        OrderNumber = string.Join("; ", package.Products.Select(a => a.OrderNumber).Distinct()),
                        Tracking = string.Join("; ", package.Products.Select(a => a.Tracking).Distinct()),
                        Notes = string.Join("; ", package.Products.Select(a => a.Notes).Distinct()),
                        package.Status,
                        package.UserCode,
                        package.SubStatus,
                        package.WeightEst,
                        package.Weight,
                        package.Cost,
                        package.IDOther,
                        package.Channel,
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
                            package.Address.IDCard,
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
                            a.Channel,
                        }),
                    };
                    return Json(newObj, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        public ActionResult CheckTracking(string id)
        {
            using (var packageDataConext = new PackageDataContext())
            {
                var packages = packageDataConext.Packages.Where(a => a.UserCode == User.Identity.GetUserCode()).ToList();
                if (packages.Select(a => String.Join("; ", a.Products.Select(b => b.Tracking).Distinct())).Contains(id))
                {
                    return Json("exist", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("not exist", JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckPackageID(string id)
        {
            using (var packageDataConext = new PackageDataContext())
            {
                var packages = packageDataConext.Packages.Where(a => a.ID == id).ToList();
                if (packages.Count > 0)
                {
                    return Json("exist", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("not exist", JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdatePackage(Package package)
        {
            using (var packageDataConext = new PackageDataContext())
            {
                var existingPackage = packageDataConext.Packages.Where(a => a.ID == package.ID).SingleOrDefault();
                if (existingPackage != null)
                {
                    // 出库扣款 Start
                    if ((existingPackage.Status == PACKAGE_STATUS_AWAIT || existingPackage.Status == PACKAGE_STATUS_IN_WAREHOUSE) &&
                        package.Status == PACKAGE_STATUS_OUT_OF_WAREHOUSE && AppUtil.IsAdmin(User.Identity.GetUserCode()) && existingPackage.Cost == null)
                    {
                        if (package.Weight == null)
                        {
                            package.Weight = 2;
                        }
                        var unitPrice = 0d;
                        if (package.Products.Count > 0 && package.Products.Where(b => b.Channel != null).Count() > 0)
                        {
                            var channelNumber = package.Products.Max(c => c.Channel);
                            var channel = packageDataConext.Channels.Where(a => a.ID == channelNumber).First();
                            var assignedUnitPrice = channel.Pricings.Where(c => c.UserCode == package.UserCode.Trim().ToUpper()).Select(c => c.Price).SingleOrDefault();
                            unitPrice = assignedUnitPrice ?? channel.DefaultPrice.Value;
                        }
                        package.Cost = package.Cost ?? package.Weight.Value * unitPrice;
                        new TransactionController().SaveTransaction(package.UserCode.Trim().ToUpper(),
                            TransactionController.TRANSACTION_TYPE_EXPENSE_SHIPPING, package.Cost * -1 ?? 0, package.ID);
                    }
                    // 出库扣款 End

                    existingPackage.Channel = package.Channel;
                    existingPackage.AddressID = package.AddressID;
                    existingPackage.SenderID = package.SenderID;
                    existingPackage.Status = package.Status;
                    existingPackage.SubStatus = package.SubStatus;
                    existingPackage.Weight = package.Weight;
                    existingPackage.Cost = package.Cost;
                    existingPackage.WeightEst = package.WeightEst ?? 2;
                    packageDataConext.Products.DeleteAllOnSubmit(existingPackage.Products);
                    existingPackage.Products.Clear();
                    existingPackage.Products.AddRange(package.Products);
                    existingPackage.IDOther = package.IDOther;
                    existingPackage.LastUpdateTime = DateTime.Now;
                    existingPackage.LastUpdateUser = User.Identity.Name;
                    packageDataConext.SubmitChanges();
                }
                else
                {
                    if (package.ID == null || package.ID == "")
                    {
                        package.ID = GeneratePackageCode();
                    }
                    package.UserCode = package.UserCode ?? User.Identity.GetUserCode();
                    package.UserCode = package.UserCode.Trim().ToUpper();
                    if (package.Sender != null)
                    {
                        package.Sender = packageDataConext.Senders.Where(a => a.ID == package.Sender.ID).SingleOrDefault();
                    }
                    if (package.Address != null)
                    {
                        package.Address = packageDataConext.Addresses.Where(a => a.ID == package.Address.ID).SingleOrDefault();
                    }
                    package.WeightEst = package.WeightEst ?? 2;
                    package.LastUpdateTime = DateTime.Now;
                    package.LastUpdateUser = User.Identity.Name;
                    packageDataConext.Packages.InsertOnSubmit(package);
                    packageDataConext.SubmitChanges();
                }
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
                if (packages[0].Sender != null)
                {
                    newPackage.Sender = packageDataConext.Senders.Where(a => a.ID == packages[0].Sender.ID).SingleOrDefault();
                }
                if (packages[0].Address != null)
                {
                    newPackage.Address = packageDataConext.Addresses.Where(a => a.ID == packages[0].Address.ID).SingleOrDefault();
                }
                newPackage.Products.AddRange(packages.SelectMany(a => a.Products));
                newPackage.Status = "待入库";
                newPackage.SubStatus = "待入库";
                newPackage.LastUpdateTime = DateTime.Now;
                newPackage.WeightEst = packages.Max(a => a.WeightEst ?? 2);
                newPackage.ID = GeneratePackageCode();
                newPackage.UserCode = packages[0].UserCode;
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
                var result = packageDataConext.Channels.GroupBy(a => a.Port).Select(a => new
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

        [AllowAnonymous]
        public ActionResult Tracking(string id)
        {
            using (var dataContext = new PackageDataContext())
            {
                var package = dataContext.Packages.Where(a => a.ID == id).SingleOrDefault();
                if (package != null)
                {
                    if (package.IDOther != null && package.IDOther.IndexOf("FH") == 0)
                    {
                        return Json(MFUtil.GetTrackingInfo(package.IDOther, package.ID), JsonRequestBehavior.AllowGet);
                    }
                    if (package.ID.IndexOf("MT") == 0)
                    {
                        return Json(YDUtil.GetTrackingInfo(id), JsonRequestBehavior.AllowGet);
                    }
                    if (package.Products.Where(a => a.OrderNumber != null && a.OrderNumber.IndexOf("HM") == 0).Count() > 0)
                    {
                        return Json(HMUtil.GetTrackingInfo(package.Products.Where(a => a.OrderNumber.IndexOf("HM") == 0).Select(a => a.OrderNumber).FirstOrDefault()), JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(TMUtil.GetTrackingInfo(id), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExportPackages(string[] ids, string template)
        {
            using (var packageDataConext = new PackageDataContext())
            {
                var packages = ids.Join(packageDataConext.Packages.Where(a => a.Address != null && a.Sender != null), a => a, b => b.ID, (a, b) => b).ToArray();
                byte[] fileContent;
                if (template == "实重收费格式")
                {
                    fileContent = MFUtil.ExportXLSOpenXML(packages);
                    return File(fileContent, AppUtil.GetContentType("xxx.xlsx"), "packages.xlsx");
                }
                else if (template == "混装格式")
                {
                    fileContent = YDUtil.ExportXLSOpenXML(packages);
                }
                else
                {
                    fileContent = TMUtil.ExportXLSOpenXML(packages);
                }
                return File(fileContent, AppUtil.GetContentType("xxx.xls"), "packages.xls");
            }
        }

        [Authorize(Users = MvcApplication.ADMIN_USERS)]
        public ActionResult GetPackageCost(string id, double weight = 2)
        {
            using (var packageDataConext = new PackageDataContext())
            {
                var existingPackage = packageDataConext.Packages.Where(a => a.ID == id).SingleOrDefault();
                if (existingPackage != null)
                {
                    // 出库扣款 实重
                    var unitPrice = 0d;
                    if (existingPackage.Products.Count > 0 && existingPackage.Products.Where(b => b.Channel != null).Count() > 0)
                    {
                        var channelNumber = existingPackage.Products.Max(c => c.Channel);
                        var channel = packageDataConext.Channels.Where(a => a.ID == channelNumber).First();
                        var assignedUnitPrice = channel.Pricings.Where(c => c.UserCode == existingPackage.UserCode).Select(c => c.Price).SingleOrDefault();
                        unitPrice = assignedUnitPrice ?? channel.DefaultPrice.Value;
                    }
                    var cost = weight * unitPrice;
                    return Json(cost, JsonRequestBehavior.AllowGet);
                }
            }
            return null;
        }

        public ActionResult PrintPackages(string[] ids)
        {
            using (var dataContext = new PackageDataContext())
            {
                var paramDict = ids.Select((a, index) => new string[] { "ids[" + index + "]", a }).ToDictionary(a => a[0], a => a[1]);
                var pdfContent = AppUtil.PostUrl("http://65.182.182.141:8888/Package/PrintPackagesPDF", paramDict);

                string fileName = "package.pdf";
                return File(pdfContent, AppUtil.GetContentType(fileName), fileName);
            }
        }

        [AllowAnonymous]
        public ActionResult PrintPackagesPDF(string[] ids)
        {
            using (var dataContext = new PackageDataContext())
            {
                var packages = ids.Join(dataContext.Packages.Where(a => a.Address != null && a.Sender != null), a => a, b => b.ID, (a, b) => b).ToList();


                // Create new document
                var document = new PdfDocument();

                // Set font encoding to unicode always
                var options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
                // Then use the font with the most language support
                var font = new XFont("Arial Unicode MS", 12, XFontStyle.Regular, options);

                var fontHeight = font.Height;

                PdfPage page = null;
                XGraphics gfx = null;
                foreach (var package in packages)
                {
                    var x = 50d;
                    var y = 20d;
                    var lineSpace = 5;
                    var offset = 15;

                    if (packages.IndexOf(package) % 2 == 0)
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                    }
                    else
                    {
                        y = page.Height / 2 + 20;
                    }

                    var barcodeImg = AppUtil.GetBarcodeImage(package.ID);


                    gfx.DrawImage(barcodeImg, x, y);
                    y += barcodeImg.Height - 10;

                    var tf = new XTextFormatter(gfx);
                    tf.Alignment = XParagraphAlignment.Left;
                    tf.DrawString(package.ID, new XFont("Arial Unicode MS", 9, XFontStyle.Regular, options), XBrushes.Black,
                      new XRect(x, y, page.Width - 2 * x, fontHeight), XStringFormats.TopLeft);
                    y += fontHeight + offset;


                    var channel = package.Channel;
                    tf.DrawString(channel, font, XBrushes.Black,
                      new XRect(x, y, page.Width - 2 * x, fontHeight), XStringFormats.TopLeft);
                    y += fontHeight + offset;


                    tf.DrawString("发件人:", font, XBrushes.Black,
                      new XRect(x, y, page.Width - 2 * x, fontHeight), XStringFormats.TopLeft);
                    y += fontHeight + lineSpace;

                    var sender = string.Join(" ", new string[] { package.Sender.Name });
                    tf.DrawString(sender, font, XBrushes.Black,
                      new XRect(x, y, page.Width - 2 * x, fontHeight), XStringFormats.TopLeft);
                    y += fontHeight + offset;

                    tf.DrawString("收件人:", font, XBrushes.Black,
                      new XRect(x, y, page.Width - 2 * x, fontHeight), XStringFormats.TopLeft);
                    y += fontHeight + lineSpace;

                    var recipient = string.Join(" ", new string[] {
                        package.Address.Name, package.Address.Phone,
                        package.Address.District1.District1.District1.Name,
                        package.Address.District1.District1.Name,
                        package.Address.District1.Name,
                        package.Address.Street
                    });
                    tf.DrawString(recipient, font, XBrushes.Black,
                      new XRect(x, y, page.Width - 2 * x, fontHeight), XStringFormats.TopLeft);
                    y += fontHeight + offset;


                    tf.DrawString("物品:", font, XBrushes.Black,
                     new XRect(x, y, page.Width - 2 * x, fontHeight), XStringFormats.TopLeft);
                    y += fontHeight + lineSpace;

                    foreach (var product in package.Products)
                    {
                        var productStr = string.Join(" ", new string[] {
                            product.Brand, product.Name, "*" + product.Quantity,
                        });
                        tf.DrawString(productStr, font, XBrushes.Black,
                          new XRect(x, y, page.Width - 2 * x, fontHeight), XStringFormats.TopLeft);
                        y += fontHeight + 2;
                    }
                }
                string filename = "package" + User.Identity.GetUserCode() + ".pdf";
                // Save the document
                document.Save(filename);
                return File(System.IO.File.ReadAllBytes(filename), AppUtil.GetContentType(filename), "package.pdf");
            }
        }


        public string GeneratePackageCode(string usercode = null)
        {
            usercode = usercode ?? User.Identity.GetUserCode();
            var code = "";
            using (var packageDataContext = new PackageDataContext())
            {
                var codeBase = "MT" + DateTime.Today.Year.ToString().Substring(2) + usercode;
                var existingCodes = packageDataContext.Packages.Select(a => a.ID).Where(a => a.Contains(codeBase)).ToArray();
                var maxCode = existingCodes.Length == 0 ? codeBase + "10000" : existingCodes.Max(a => a);
                code = codeBase + (long.Parse(maxCode.Replace(codeBase, "")) + 1);
            }
            return code;
        }

        public void CreateTMEntry()
        {
            var package = new Package
            {
                Sender = new Sender
                {
                    Name = "Xiao",
                    Phone = "5553331222",
                    Address = "test"
                },
                Address = new PackageDataContext().Addresses.Where(a => a.District == 41).SingleOrDefault()
            };
            package.Products.Add(new Product {
                Name = "test",
                Brand = "Coach",
                Price = 134,
                Quantity = 1,
                Channel1 = new PackageDataContext().Channels.Where(a => a.DefaultPrice == 3.5).First(),
            });
        }

        public double RoundPackageWeight(double weight)
        {
            if (weight < 2) { return 2; }
            return Math.Floor(weight) + (weight - Math.Floor(weight) > 0.1 ? 1 : 0);
        }

        public void GetPackageWeight(string id)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                var packages = packageDataContext.Packages.Where(a => a.Status != PACKAGE_STATUS_NO_NOTICE &&
                    a.Status != PACKAGE_STATUS_AWAIT && a.Status != PACKAGE_STATUS_IN_WAREHOUSE).ToList();
                if (id != null)
                {
                    packages = packages.Where(a => a.ID == id).ToList();
                }
                foreach (var package in packages)
                {
                    double? weight, cost;
                    TMUtil.GetPackageWeightAndCost(package.ID, out weight, out cost);
                    package.Weight = weight;
                }
                packageDataContext.SubmitChanges();
            }
        }


        public ActionResult Intro()
        {
            ViewBag.angularAppName = "packageApp";
            ViewBag.angularControllerName = "packageCtrl";
            return View();
        }


        public const string PACKAGE_STATUS_NO_NOTICE = "未预报";
        public const string PACKAGE_STATUS_AWAIT = "待入库";
        public const string PACKAGE_STATUS_IN_WAREHOUSE = "已入库";
        public const string PACKAGE_STATUS_OUT_OF_WAREHOUSE = "已出库";
        public const string PACKAGE_STATUS_ON_PLAIN = "空运";
        public const string PACKAGE_STATUS_CUSTOMS_CLEARANCE = "清关中";
        public const string PACKAGE_STATUS_DOMESTIC_TRANSPORTATION = "国内转运";

        public const double PACKAGE_WEIGHT_ESTIMATE_DEFAULT = 2d;
    }
}