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
    public class MessageController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.angularAppName = "messageApp";
            ViewBag.angularControllerName = "messageCtrl";
            return View();
        }

        public ActionResult GetMessages(string id, string status)
        {
            var isAdmin = AppUtil.IsAdmin(User.Identity.GetUserCode());
            if (!isAdmin)
            {
                id = User.Identity.GetUserCode();
            }
            if (id != null) { isAdmin = false; }
            using (var packageDataContext = new PackageDataContext())
            {
                var messages = packageDataContext.Messages.Where(a => (id == null || a.UserCode == id) && (status == "全部" || a.Status == status)).ToList().Select(a => new
                {
                    a.ID,
                    a.UserCode,
                    a.Category,
                    a.Tracking,
                    Comment = a.MessageContents.Select(b => b.Comment).FirstOrDefault(),
                    LastComment = a.MessageContents.Select(b => b.Comment).LastOrDefault(),
                    Timestamp = a.MessageContents.Select(b => b.Timestamp).LastOrDefault().ToString("MM/dd/yyyy hh:mm tt"),
                    a.Status,
                }).ToList();
                return Json(messages, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetMessage(int id)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                var message = packageDataContext.Messages.Where(a => a.ID == id).SingleOrDefault();
                if (message == null) { return null; }
                var result = new
                {
                    message.ID,
                    message.Category,
                    message.UserCode,
                    message.Status,
                    message.Tracking,
                    Comment = message.MessageContents.Select(b => b.Comment).FirstOrDefault(),
                    Timestamp = message.MessageContents.Count == 0 ? "" : message.MessageContents.Max(b => b.Timestamp).ToString("MM/dd/yyyy hh:mm tt"),
                    MessageContents = message.MessageContents.Select(a => new
                    {
                        a.ID,
                        a.MessageID,
                        a.Sender,
                        a.Comment,
                        Timestamp = a.Timestamp.ToString("MM/dd/yyyy hh:mm tt"),
                    }).ToList()
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteMessage(long id)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                var existingMessage = packageDataContext.Messages.Where(a => a.ID == id).SingleOrDefault();
                if (existingMessage != null)
                {
                    if (AppUtil.IsAdmin(User.Identity.GetUserCode()) || existingMessage.UserCode == User.Identity.GetUserCode())
                    {
                        packageDataContext.Messages.DeleteOnSubmit(existingMessage);
                        packageDataContext.SubmitChanges();
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        public ActionResult CloseMessage(long id)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                var existingMessage = packageDataContext.Messages.Where(a => a.ID == id).SingleOrDefault();
                if (existingMessage != null)
                {
                    existingMessage.Status = "已解决";
                    packageDataContext.SubmitChanges();
                }
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        

        public ActionResult PostMessage(Message message, string messageStr)
        {
            using(var packageDataContext = new PackageDataContext())
            {
                var existingMessage = packageDataContext.Messages.Where(a => a.ID == message.ID).SingleOrDefault();
                if (existingMessage != null)
                {
                    existingMessage.Category = message.Category;
                    existingMessage.Tracking = message.Tracking;
                    if (messageStr != null && messageStr.Trim() != "")
                    {
                        var messageContent = new MessageContent
                        {
                            Comment = messageStr,
                            Sender = User.Identity.GetUserCode(),
                            Timestamp = DateTime.Now,
                        };

                        existingMessage.MessageContents.Add(messageContent);
                        if (messageContent.Sender != existingMessage.UserCode)
                        {
                            existingMessage.Status = "已回复";
                        }
                    }
                    packageDataContext.SubmitChanges();
                }
                else
                {
                    message.UserCode = User.Identity.GetUserCode();
                    message.MessageContents.Add(new MessageContent
                    {
                        Comment = messageStr,
                        Sender = User.Identity.GetUserCode(),
                        Timestamp = DateTime.Now,
                    });
                    packageDataContext.Messages.InsertOnSubmit(message);
                    packageDataContext.SubmitChanges();
                }
            }
            return GetMessage(message.ID);
        }

        public ActionResult DeleteMessageContent(long id)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                var existingMessageContent = packageDataContext.MessageContents.Where(a => a.ID == id).SingleOrDefault();
                if (existingMessageContent != null)
                {
                    if (AppUtil.IsAdmin(User.Identity.GetUserCode()) || existingMessageContent.Message.UserCode == User.Identity.GetUserCode())
                    {
                        packageDataContext.MessageContents.DeleteOnSubmit(existingMessageContent);
                        packageDataContext.SubmitChanges();
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
    }
}