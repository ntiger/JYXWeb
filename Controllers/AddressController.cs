using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using JYXWeb.DB;
using JYXWeb.Util;

namespace JYXWeb.Controllers
{
    [Authorize]
    public class AddressController : Controller
    {
        public ActionResult GetDistricts(int? id)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                var districts = packageDataContext.Districts.Where(a => a.ParentID == id || !id.HasValue && !a.ParentID.HasValue).Select(a => new
                {
                    a.ID,
                    a.Name
                }).ToList();
                return Json(districts, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetAddresses(int? id)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                var addresses = packageDataContext.Addresses.Where(a => a.UserCode == User.Identity.GetUserCode() || a.ID == id).Select(a => new
                {
                    a.ID,
                    ProvinceName = a.District1.District1.District1.Name,
                    Province = a.District1.District1.District1.ID,
                    CityName = a.District1.District1.Name,
                    City = a.District1.District1.ID,
                    DistrictName = a.District1.Name,
                    District = a.District,
                    Street = a.Street,
                    a.Name,
                    a.Phone,
                    a.PostCode,
                    a.IDCard,
                    AddressIDCardImages = a.AddressIDCardImages.Select(b => new
                    {
                        b.Image
                    }).ToList(),
                    a.IsDefault,
                }).OrderBy(a => a.IsDefault).ToList();
                return Json(addresses, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateAddress(Address address)
        {
            for (var i = 0; i < address.AddressIDCardImages.Count; i++)
            {
                var img = address.AddressIDCardImages[i].Image;
                var commaIndex = address.AddressIDCardImages[i].Image.IndexOf(",");
                var header = "data:image/jpeg;base64,";
                if (commaIndex > -1)
                {
                    header = img.Substring(0, commaIndex + 1);
                    img = img.Substring(commaIndex + 1);
                }
                address.AddressIDCardImages[i].Image = header + AppUtil.ResizeImage(img, 800);
            }
            using (var packageDataContext = new PackageDataContext())
            {
                var existingAddress = packageDataContext.Addresses.Where(a => a.ID == address.ID).SingleOrDefault();
                if (existingAddress != null)
                {
                    existingAddress.Name = address.Name;
                    existingAddress.District = address.District;
                    existingAddress.Phone = address.Phone;
                    existingAddress.PostCode = address.PostCode;
                    existingAddress.IDCard = address.IDCard;
                    existingAddress.IsDefault = address.IsDefault;
                    packageDataContext.AddressIDCardImages.DeleteAllOnSubmit(existingAddress.AddressIDCardImages);
                    existingAddress.AddressIDCardImages.Clear();
                    existingAddress.AddressIDCardImages.AddRange(address.AddressIDCardImages);
                }
                else
                {
                    address.UserCode = User.Identity.GetUserCode();
                    packageDataContext.Addresses.InsertOnSubmit(address);
                }

                packageDataContext.SubmitChanges();
            }
            return null;
        }

        public ActionResult DeleteAddress(int id)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                var returnStr = "";
                var address = packageDataContext.Addresses.Where(a => a.ID == id).SingleOrDefault();
                if (address != null)
                {
                    if (address.UserCode == User.Identity.GetUserCode())
                    {
                        packageDataContext.Addresses.DeleteOnSubmit(address);
                        packageDataContext.SubmitChanges();
                    }
                    else
                    {
                        returnStr = "无法删除他人地址";
                    }
                }
                else
                {
                    returnStr = "地址不存在";
                }
                return Json(returnStr, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetSenders(int? id)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                var senders = packageDataContext.Senders.Where(a => a.UserCode == User.Identity.GetUserCode() || a.ID == id).Select(a => new
                {
                    a.ID,
                    a.Name,
                    a.Phone,
                    a.Address,
                    a.IsDefault
                }).OrderBy(a => a.IsDefault).ToList();
                return Json(senders, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult UpdateSender(Sender sender)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                var existingSenders = packageDataContext.Senders.Where(a => a.ID == sender.ID).SingleOrDefault();
                if (existingSenders != null)
                {
                    existingSenders.Name = sender.Name;
                    existingSenders.Phone = sender.Phone;
                    existingSenders.Address = sender.Address;
                    existingSenders.IsDefault = sender.IsDefault;
                }
                else
                {
                    sender.UserCode = User.Identity.GetUserCode();
                    packageDataContext.Senders.InsertOnSubmit(sender);
                }

                packageDataContext.SubmitChanges();
            }
            return null;
        }

        public ActionResult DeleteSender(int id)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                var returnStr = "";
                var sender = packageDataContext.Senders.Where(a => a.ID == id).SingleOrDefault();
                if (sender != null)
                {
                    if (sender.UserCode == User.Identity.GetUserCode())
                    {
                        packageDataContext.Senders.DeleteOnSubmit(sender);
                        packageDataContext.SubmitChanges();
                    }
                    else
                    {
                        returnStr = "无法删除他人的发件人";
                    }
                }
                else
                {
                    returnStr = "发件人不存在";
                }
                return Json(returnStr, JsonRequestBehavior.AllowGet);
            }
        }
    }
}