using JYXWeb.DB;
using JYXWeb.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace JYXWeb.Controllers
{
    [Authorize]
    public class PurchaseController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.angularAppName = "purchaseApp";
            ViewBag.angularControllerName = "purchaseCtrl";
            return View();
        }

        public ActionResult GetOrders(string userCode, string status)
        {
            var isAdmin = AppUtil.IsAdmin(User.Identity.GetUserCode());
            if (!isAdmin)
            {
                userCode = User.Identity.GetUserCode();
            }
            if (userCode != null) { isAdmin = false; }

            using (var dataContext = new PackageDataContext())
            {
                var orders = dataContext.PurchaseOrders.Where(a => (userCode == null || a.ID.Contains(userCode)) && (status == "全部" || a.Status == status)).ToList().Select(a => new
                {
                    a.ID,
                    a.CreateTime,
                    a.Name,
                    a.Quantity,
                    a.Price,
                    a.Notes,
                    a.Status,
                });

                return Json(orders);
            }
        }

        public ActionResult GetOrder(string id)
        {
            var userCode = User.Identity.GetUserCode();
            using (var dataContext = new PackageDataContext())
            {
                var order = dataContext.PurchaseOrders.Where(a => a.ID == id && (AppUtil.IsAdmin(userCode) || id.Contains(userCode))).SingleOrDefault();
                if (order != null)
                {
                    var result = new
                    {
                        order.ID,
                        order.Name,
                        order.Link,
                        order.Quantity,
                        order.Price,
                        order.Notes,
                        order.Color,
                        order.Size,
                        order.PercentFee,
                        order.Status,
                        PurchaseOrderImages = order.PurchaseOrderImages.Select(b => new
                        {
                            b.Image
                        }).ToList(),
                    };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                return Json(null);
            }
        }

        public ActionResult UpdateOrder(PurchaseOrder order)
        {
            for (var i = 0; i < order.PurchaseOrderImages.Count; i++)
            {
                var img = order.PurchaseOrderImages[i].Image;
                var commaIndex = order.PurchaseOrderImages[i].Image.IndexOf(",");
                var header = "data:image/jpeg;base64,";
                if (commaIndex > -1)
                {
                    header = img.Substring(0, commaIndex + 1);
                    img = img.Substring(commaIndex + 1);
                }
                order.PurchaseOrderImages[i].Image = header + AppUtil.ResizeImage(img, 800);
            }
            var id = order.ID;
            using (var dataContext = new PackageDataContext())
            {
                var existingRecord = dataContext.PurchaseOrders.Where(a => a.ID == order.ID).SingleOrDefault();
                if (existingRecord != null)
                {
                    existingRecord.Link = order.Link;
                    existingRecord.Name = order.Name;
                    existingRecord.Notes = order.Notes;
                    existingRecord.Color = order.Color;
                    existingRecord.Size = order.Size;
                    existingRecord.Quantity = order.Quantity;
                    existingRecord.Price = order.Price;
                    existingRecord.PercentFee = order.PercentFee;
                    existingRecord.PurchaseOrderImages = order.PurchaseOrderImages;
                    existingRecord.LastUpdateTime = DateTime.Now;
                    existingRecord.Status = order.Status;
                    if (order.Status == PURCHASE_ORDER_STATUS_CANCELLED)
                    {
                        dataContext.Products.DeleteAllOnSubmit(dataContext.Products.Where(a => a.Tracking == existingRecord.ID));
                        dataContext.SubmitChanges();
                        dataContext.Packages.DeleteAllOnSubmit(dataContext.Packages.Where(a => a.UserCode == User.Identity.GetUserCode() && a.Products.Count == 0));
                        dataContext.SubmitChanges();
                        if (existingRecord.Quantity != null && existingRecord.Price != null)
                        {
                            new TransactionController().SaveTransaction(id.Substring(0, 6), TransactionController.TRANSACTION_TYPE_REFUND_PURCHASE, existingRecord.Quantity.Value * existingRecord.Price.Value, "取消代刷订单：" + id);
                        }
                    }
                }
                else
                {
                    var userCode = User.Identity.GetUserCode();
                    var lastID = dataContext.PurchaseOrders.Where(a => a.ID.Contains(userCode)).Select(a => a.ID).OrderByDescending(a => a).FirstOrDefault();
                    order.ID = userCode + (lastID == null ? 100000 : int.Parse(lastID.Replace(userCode, "").Replace(PURCHASE_ORDER_ID_TAIL, "")) + 1) + PURCHASE_ORDER_ID_TAIL;
                    order.CreateTime = DateTime.Now;
                    order.LastUpdateTime = DateTime.Now;
                    dataContext.PurchaseOrders.InsertOnSubmit(order);
                    id = order.ID;

                    if (order.Quantity != null && order.Price != null)
                    {
                        new TransactionController().SaveTransaction(id.Substring(0, 6), TransactionController.TRANSACTION_TYPE_REFUND_PURCHASE, -1 * order.Quantity.Value * order.Price.Value * (1 + order.PercentFee.Value), "代刷订单：" + id);
                    }
                }
                dataContext.SubmitChanges();
            }

            return Json(id);
        }

        public ActionResult DeleteOrder(string id)
        {
            var userCode = User.Identity.GetUserCode();
            using (var dataContext = new PackageDataContext())
            {
                if (id != null && id.Contains(userCode)|| AppUtil.IsAdmin(userCode))
                {
                    var purchaseOrder = dataContext.PurchaseOrders.Where(a => a.ID == id).SingleOrDefault();
                    if (purchaseOrder != null) {
                        

                        dataContext.PurchaseOrders.DeleteOnSubmit(purchaseOrder);
                        dataContext.Products.DeleteAllOnSubmit(dataContext.Products.Where(a => a.Tracking == id));
                        dataContext.SubmitChanges();
                        dataContext.Packages.DeleteAllOnSubmit(dataContext.Packages.Where(a => a.UserCode == userCode && a.Products.Count == 0));
                        dataContext.SubmitChanges();
                    }
                }
            }
            return null;
        }

        public ActionResult CreatePackage(string id)
        {
            using (var dataContext = new PackageDataContext())
            {
                if (id != null && id.Contains(User.Identity.GetUserCode()))
                {
                    var order = dataContext.PurchaseOrders.Where(a => a.ID == id).SingleOrDefault();
                    if (order != null)
                    {

                        dataContext.Products.DeleteAllOnSubmit(dataContext.Products.Where(a => a.Tracking == id));
                        dataContext.SubmitChanges();
                        dataContext.Packages.DeleteAllOnSubmit(dataContext.Packages.Where(a => a.UserCode == User.Identity.GetUserCode() && a.Products.Count == 0));
                        dataContext.SubmitChanges();

                        var package = new Package
                        {
                            Status = PackageController.PACKAGE_STATUS_AWAIT,
                            SubStatus = PackageController.PACKAGE_STATUS_AWAIT,
                            WeightEst = PackageController.PACKAGE_WEIGHT_ESTIMATE_DEFAULT,
                        };
                        package.Products.Add(new Product
                        {
                            Tracking = order.ID,
                            Name = order.Name,
                            Channel = 9,
                            Quantity = order.Quantity,
                            Price = order.Price,
                            Notes = order.Notes,
                        });
                        var factory = DependencyResolver.Current.GetService<IControllerFactory>() ?? new DefaultControllerFactory();
                        var packageController = factory.CreateController(this.ControllerContext.RequestContext, "Package") as PackageController;
                        var route = new RouteData();
                        var newContext = new ControllerContext(new HttpContextWrapper(System.Web.HttpContext.Current), route, packageController);
                        packageController.ControllerContext = newContext;
                        packageController.UpdatePackage(package);
                    }
                }
            }
            return null;
        }


        public const string PURCHASE_ORDER_ID_TAIL = "DS";

        public const string PURCHASE_ORDER_STATUS_SUBMITTED = "已提交";
        public const string PURCHASE_ORDER_STATUS_COMPLETED = "已购买";
        public const string PURCHASE_ORDER_STATUS_CANCELLED = "已取消";
    }
}