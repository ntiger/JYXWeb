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
        public ActionResult GetTransactions(string id)
        {
            var userCode = id ?? User.Identity.GetUserCode();
            using (var dataContext = new PackageDataContext())
            {
                var transactions = dataContext.Transactions.Where(a => a.UserCode == userCode).OrderByDescending(a => a.Timestamp).ToList();
                return Json(transactions, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Deposit(Deposit deposit)
        {
            using(var dataContext = new PackageDataContext())
            {
                deposit.InputTimestamp = DateTime.Now;
                deposit.Status = DEPOSIT_STATUS_PENDING;
                deposit.UserCode = User.Identity.GetUserCode();
                deposit.DepositAmount = Math.Round(deposit.InputAmount / CURRENCY_RATE, 2);
                dataContext.Deposits.InsertOnSubmit(deposit);
                dataContext.SubmitChanges();
            }
            return null;
        }

        public ActionResult ConfirmDeposit(Deposit deposit)
        {
            using (var dataContext = new PackageDataContext())
            {
                var existingRecord = dataContext.Deposits.Where(a => a.ID == deposit.ID).SingleOrDefault();
                if (existingRecord != null)
                {
                    existingRecord.VerifyTimestamp = DateTime.Now;
                    existingRecord.Status = DEPOSIT_STATUS_FINISHED;
                    existingRecord.DepositAmount = deposit.DepositAmount;
                    dataContext.SubmitChanges();
                    SaveTransaction(existingRecord.UserCode, TRANSACTION_TYPE_DEPOSIT_ALIPAY, existingRecord.DepositAmount, existingRecord.Reference);
                }
            }
            return null;
        }

        public ActionResult CancelDeposit(Deposit deposit)
        {
            using (var dataContext = new PackageDataContext())
            {
                var existingRecord = dataContext.Deposits.Where(a => a.ID == deposit.ID).SingleOrDefault();
                if (existingRecord != null)
                {
                    existingRecord.VerifyTimestamp = DateTime.Now;
                    existingRecord.Status = DEPOSIT_STATUS_CANCELLED;
                    dataContext.SubmitChanges();
                }
            }
            return null;
        }

        public ActionResult GetDeposits(string id)
        {
            var userCode = id ?? User.Identity.GetUserCode();
            using (var dataContext = new PackageDataContext())
            {
                var deposits = dataContext.Deposits.Where(a => a.UserCode == userCode).OrderByDescending(a => a.InputTimestamp).ToList();
                return Json(deposits, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Users = MvcApplication.ADMIN_USERS)]
        public ActionResult SaveTransaction(string userCode, string type, double amount, string description)
        {
            using (var dataContext = new PackageDataContext())
            {
                var lastTransaction = dataContext.Transactions.Where(a => a.UserCode == userCode).OrderByDescending(a => a.ID).FirstOrDefault();
                var balance = lastTransaction == null ? 0 : lastTransaction.Balance;
                dataContext.Transactions.InsertOnSubmit(new Transaction
                {
                    Type = type,
                    Amount = amount,
                    Description = description,
                    UserCode = userCode,
                    UpdateUser = User == null ? "System" : User.Identity.GetUserCode(),
                    Balance = balance + amount,
                    Timestamp = DateTime.Now
                });
                dataContext.SubmitChanges();
            }
            return Json("Success");
        }

        [Authorize(Users = MvcApplication.ADMIN_USERS)]
        public ActionResult ManualDeposit(double amount, string userCode, string memo)
        {
            SaveTransaction(userCode, TRANSACTION_TYPE_DEPOSIT_MANUAL, amount, memo);
            return null;
        }

        public const double CURRENCY_RATE = 7;

        public const string DEPOSIT_STATUS_PENDING = "待处理";
        public const string DEPOSIT_STATUS_FINISHED = "已处理";
        public const string DEPOSIT_STATUS_CANCELLED = "已取消";

        public const string TRANSACTION_TYPE_DEPOSIT_MANUAL = "手动充值";
        public const string TRANSACTION_TYPE_DEPOSIT_ALIPAY = "支付宝充值";
        public const string TRANSACTION_TYPE_EXPENSE_SHIPPING = "运费支出";
        public const string TRANSACTION_TYPE_EXPENSE_GOODS = "代购支出";
        public const string TRANSACTION_TYPE_EXPENSE_PURCHASE = "代刷支出";
        public const string TRANSACTION_TYPE_REFUND_SHIPPING = "运费支出";
        public const string TRANSACTION_TYPE_REFUND_GOODS = "代购退款";
        public const string TRANSACTION_TYPE_REFUND_PURCHASE = "代刷退款";

    }
}