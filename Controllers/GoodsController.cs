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

        public ActionResult Update(Good goods)
        {
            for (var i = 0; i < goods.GoodsItems.Count; i++)
            {
                for(var j = 0; j < goods.GoodsItems[i].GoodsImages.Count; j++)
                {
                    var img = goods.GoodsItems[i].GoodsImages[j].Image;
                    var commaIndex = goods.GoodsItems[i].GoodsImages[j].Image.IndexOf(",");
                    var header = "data:image/jpeg;base64,";
                    if (commaIndex > -1)
                    {
                        header = img.Substring(0, commaIndex + 1);
                        img = img.Substring(commaIndex + 1);
                    }
                    goods.GoodsItems[i].GoodsImages[j].Image = header + AppUtil.ResizeImage(img, 800);
                }
            }
            using (var dataContext = new PackageDataContext())
            {
                var existingRecord = dataContext.Goods.Where(a => a.ID == goods.ID).SingleOrDefault();
                if (existingRecord != null)
                {
                    existingRecord.Category = goods.Category;
                    existingRecord.Brand = goods.Brand;
                    existingRecord.Name = goods.Name;
                    existingRecord.Notes = goods.Notes;
                    foreach(var item in existingRecord.GoodsItems)
                    {
                        dataContext.GoodsImages.DeleteAllOnSubmit(item.GoodsImages);
                    }
                    dataContext.GoodsItems.DeleteAllOnSubmit(existingRecord.GoodsItems);
                    existingRecord.GoodsItems.Clear();
                    existingRecord.GoodsItems.AddRange(goods.GoodsItems);
                    existingRecord.LastUpdateTime = DateTime.Now;
                    existingRecord.LastUpdateUser = User.Identity.GetUserCode();
                }
                else
                {
                    goods.LastUpdateTime = DateTime.Now;
                    goods.LastUpdateUser = User.Identity.GetUserCode();
                    dataContext.Goods.InsertOnSubmit(goods);
                }
                dataContext.SubmitChanges();
            }
            return null;
        }

        public ActionResult Search(string category)
        {
            using (var dataContext = new PackageDataContext())
            {
                var goods = dataContext.Goods.Where(a => category == "所有商品" || a.Category == category).ToList().Select(a => new
                {
                    a.ID,
                    a.Category,
                    a.Brand,
                    a.Name,
                    GoodsItems = a.GoodsItems.Select(b => new
                    {
                        b.Price,
                        b.Quantity,
                        b.Size,
                        b.Color,
                        GoodsImages = b.GoodsImages.Select(c => new
                        {
                            Image = c.Image,
                        }).Take(1).ToList(),
                    }).ToList(),
                }).OrderBy(a => a.Category).ToList();
                var result = Json(goods);
                result.MaxJsonLength = int.MaxValue;
                return result;
            }
        }

        public ActionResult GetGoodsEntry(int id)
        {
            using (var dataContext = new PackageDataContext())
            {
                var goodsEntry = dataContext.Goods.Where(a => a.ID == id).SingleOrDefault();
                if (goodsEntry != null)
                {
                    var result = new
                    {
                        goodsEntry.ID,
                        goodsEntry.Category,
                        goodsEntry.Brand,
                        goodsEntry.Name,
                        ColorSizes = goodsEntry.GoodsItems.GroupBy(a => a.Color).Select(a => new
                        {
                            Color = a.Key,
                            Sizes = a.Select(b => new { b.Size }).ToArray(),
                        }).ToList(),
                        GoodsItems = goodsEntry.GoodsItems.Select(b => new
                        {
                            b.ID,
                            b.Price,
                            b.Quantity,
                            b.Color,
                            b.Size,
                            GoodsImages = b.GoodsImages.Select(c => new
                            {
                                Image = c.Image,
                            }).ToList(),
                        }).ToList(),
                    };
                    var jsonResult = Json(result);
                    jsonResult.MaxJsonLength = int.MaxValue;
                    return jsonResult;
                }
                return null;
            }
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
                var orders = dataContext.GoodsOrders.Where(a => (userCode == null || a.ID.Contains(userCode)) && (status == "全部" || a.Status == status)).ToList().Select(a => new
                {
                    a.ID,
                    a.CreateTime,
                    Brand = a.GoodsItem.Good.Brand,
                    Name = a.GoodsItem.Good.Name,
                    ColorSize = a.GoodsItem.Color + ", " + a.GoodsItem.Size,
                    a.Quantity,
                    TotalPrice = a.GoodsItem.Price * a.Quantity,
                    a.Status,
                }).ToList();

                return Json(orders);
            }
        }

        public ActionResult GetOrder(string id)
        {
            var userCode = User.Identity.GetUserCode();
            using (var dataContext = new PackageDataContext())
            {
                var order = dataContext.GoodsOrders.Where(a => a.ID == id && (AppUtil.IsAdmin(userCode) || id.Contains(userCode))).SingleOrDefault();
                if (order != null)
                {
                    var result = new
                    {
                        order.ID,
                        order.Quantity,
                        order.Status,
                    };
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                return Json(null);
            }
        }

        public ActionResult UpdateOrder(GoodsOrder order)
        {
            var id = order.ID;
            using (var dataContext = new PackageDataContext())
            {
                var existingRecord = dataContext.GoodsOrders.Where(a => a.ID == order.ID).SingleOrDefault();
                if (existingRecord != null)
                {
                    existingRecord.Quantity = order.Quantity;
                    existingRecord.Status = order.Status;
                    existingRecord.LastUpdateTime = DateTime.Now;
                    if (order.Status == ORDER_STATUS_CANCELLED)
                    {
                        dataContext.Products.DeleteAllOnSubmit(dataContext.Products.Where(a => a.Tracking == existingRecord.ID));
                        dataContext.SubmitChanges();
                        dataContext.Packages.DeleteAllOnSubmit(dataContext.Packages.Where(a => a.UserCode == User.Identity.GetUserCode() && a.Products.Count == 0));
                        dataContext.SubmitChanges();
                        existingRecord.GoodsItem.Quantity += existingRecord.Quantity;
                        if (existingRecord.GoodsItem.Price != null && order.Quantity != null)
                        {
                            new TransactionController().SaveTransaction(order.ID.Substring(0, 6), TransactionController.TRANSACTION_TYPE_REFUND_GOODS, existingRecord.GoodsItem.Price.Value * order.Quantity.Value, "取消代购订单:" + existingRecord.ID);
                        }
                    }
                }
                else
                {
                    var userCode = User.Identity.GetUserCode();
                    var lastID = dataContext.GoodsOrders.Where(a => a.ID.Contains(userCode)).Select(a => a.ID).OrderByDescending(a => a).FirstOrDefault();
                    order.ID = userCode + (lastID == null ? 100000 : int.Parse(lastID.Replace(userCode, "")) + 1) + GOODS_ORDER_ID_TAIL;
                    order.CreateTime = DateTime.Now;
                    order.LastUpdateTime = DateTime.Now;
                    order.GoodsItem = dataContext.GoodsItems.Where(a => a.ID == order.GoodsItem.ID).SingleOrDefault();
                    dataContext.GoodsOrders.InsertOnSubmit(order);
                    order.GoodsItem.Quantity -= order.Quantity;
                    if (order.GoodsItem.Price != null && order.Quantity != null)
                    {
                        new TransactionController().SaveTransaction(order.ID.Substring(0, 6), TransactionController.TRANSACTION_TYPE_EXPENSE_GOODS, -1 * order.GoodsItem.Price.Value * order.Quantity.Value, "代购订单:" + order.ID);
                    }
                }
                dataContext.SubmitChanges();
                id = order.ID;
            }

            return Json(id);
        }

        public ActionResult DeleteOrder(string id)
        {
            var userCode = User.Identity.GetUserCode();
            using (var dataContext = new PackageDataContext())
            {
                if (id != null && id.Contains(userCode) || AppUtil.IsAdmin(userCode))
                {
                    var order = dataContext.GoodsOrders.Where(a => a.ID == id).SingleOrDefault();
                    if (order != null)
                    {
                        dataContext.GoodsOrders.DeleteOnSubmit(order);
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
                    var order = dataContext.GoodsOrders.Where(a => a.ID == id).SingleOrDefault();
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
                            Brand = order.GoodsItem.Good.Brand,
                            Name = order.GoodsItem.Good.Name,
                            Quantity = order.Quantity,
                            Price = order.GoodsItem.Price,
                            Notes = "颜色：" + order.GoodsItem.Color + ",型号" + order.GoodsItem.Size,
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

        

        public const string GOODS_ORDER_ID_TAIL = "DG";

        public const string ORDER_STATUS_SUBMITTED = "已提交";
        public const string ORDER_STATUS_COMPLETED = "已结单";
        public const string ORDER_STATUS_CANCELLED = "已取消";
    }
}