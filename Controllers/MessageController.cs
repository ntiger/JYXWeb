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

        public ActionResult GetMessages(string id)
        {
            var userCode = id ?? User.Identity.GetUserCode();
            using(var packageDataContext = new PackageDataContext())
            {
                var messages = packageDataContext.Messages.Where(a => a.UserCode == userCode).ToList().Select(a => new
                {
                    a.ID,
                    a.Category,
                    Comment = a.MessageContents.Select(b => b.Comment).FirstOrDefault(),
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
                var result = new
                {
                    message.ID,
                    message.Category,
                    message.Status,
                    MessageContents = message.MessageContents.Select(a => new
                    {
                        ID = a.ID,
                        MessageID = a.MessageID,
                        a.Comment,
                        Timestamp = a.Timestamp.ToString("MM/dd/yyyy hh:mm tt"),
                    }).ToList()
                };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult PostMessage(Message message, string messageStr)
        {
            using(var packageDataContext = new PackageDataContext())
            {
                var existingMessage = packageDataContext.Messages.Where(a => a.ID == message.ID).SingleOrDefault();
                if (existingMessage != null)
                {
                    existingMessage.MessageContents.Add(new MessageContent
                    {
                        Comment = messageStr,
                        Sender = User.Identity.GetUserCode(),
                        Timestamp = DateTime.Now,
                    });
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

    }
}