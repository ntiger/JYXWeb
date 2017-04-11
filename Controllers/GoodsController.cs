using JYXWeb.DB;
using JYXWeb.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                    existingRecord.GoodsItems = goods.GoodsItems;
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
                var goods = dataContext.Goods.Where(a => a.Category == category).Select(a => new
                {
                    a.ID,
                    a.Category,
                    a.Brand,
                    a.Name,
                    GoodsItems = a.GoodsItems.Select(b =>new
                    {
                        b.Price,
                        b.Quantity,
                        b.Size,
                        GoodsImages = b.GoodsImages.Select(c => new
                        {
                            Image = c.Image,
                        })
                    }),
                }).ToList();
                var result = Json(goods);
                result.MaxJsonLength = int.MaxValue;
                return result;
            }
        }
    }
}