using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using JYXWeb.Models;
using JYXWeb.Util;
using JYXWeb.DB;

namespace JYXWeb.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        #region Account Credential

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "用户名/邮箱或密码错误");
                    return View(model);
            }
        }


        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email, PasswordPlain = model.Password };
                var userCodes = new ApplicationDbContext().Users.Select(a => a.UserCode).ToArray();
                while (userCodes.Contains(user.UserCode) || user.UserCode == null)
                {
                    user.UserCode = AppUtil.GenerateUserCode();
                }

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                    return RedirectToAction("Index");
                }
                ModelState.AddModelError(string.Empty, "用户名/邮箱已存在");
                //AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }


        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    user.PasswordPlain = model.NewPassword;
                    await UserManager.UpdateAsync(user);
                }

                return RedirectToAction("Index", new { Message = "密码修改成功" });
            }
            ModelState.AddModelError(string.Empty, "旧密码输入错误");
            return View(model);
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Account Center

        public ActionResult Index()
        {
            ViewBag.angularAppName = "accountApp";
            ViewBag.angularControllerName = "accountCtrl";
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SaveName(string firstName, string lastName)
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                user.FirstName = firstName;
                user.LastName = lastName;
                await UserManager.UpdateAsync(user);
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return null;
        }

        [HttpPost]
        [Authorize(Users = MvcApplication.ADMIN_USERS)]
        public async Task<ActionResult> UpdateUserMemo(string id, string memo)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Memo = memo;
                await UserManager.UpdateAsync(user);
            }
            return null;
        }

        [HttpPost]
        [Authorize(Users = MvcApplication.ADMIN_USERS)]
        public async Task<ActionResult> UpdateUserType(string id, string userType)
        {
            var user = await UserManager.FindByIdAsync(id);
            if (user != null)
            {
                user.UserType = userType;
                await UserManager.UpdateAsync(user);
            }
            return null;
        }



        [Authorize(Users = MvcApplication.ADMIN_USERS)]
        public ActionResult GetUsers()
        {
            using (var appDbContext = new ApplicationDbContext())
            using (var packageDbContext = new PackageDataContext())
            {
                var users = appDbContext.Users.Select(a => new
                {
                    a.UserName,
                    a.UserCode,
                    a.FirstName,
                    a.LastName,
                    a.UserType,
                    a.Memo,
                    a.Id
                }).ToList();
                var userBalances = packageDbContext.Transactions.OrderByDescending(a => a.ID).GroupBy(a => a.UserCode).Select(a => new
                {
                    UserCode = a.Key,
                    Balance = a.Select(b => b.Balance).First(),
                }).ToList();

                var userPendingBalances = packageDbContext.Deposits.Where(a => a.Status == TransactionController.DEPOSIT_STATUS_PENDING)
                    .GroupBy(a => a.UserCode).Select(a => new
                    {
                        UserCode = a.Key,
                        PendingBalance = a.Count() == 0 ? 0 : a.Sum(b => b.DepositAmount),
                    }).ToList();

                var results = from a in users
                              from b in userBalances.Where(b => b.UserCode == a.UserCode).DefaultIfEmpty()
                              from c in userPendingBalances.Where(c => c.UserCode == a.UserCode).DefaultIfEmpty()
                              select new
                              {
                                  a.UserName,
                                  a.UserCode,
                                  a.FirstName,
                                  a.LastName,
                                  a.UserType,
                                  a.Memo,
                                  a.Id,
                                  Balance = b == null ? 0 : b.Balance,
                                  PendingBalance = c == null ? 0 : c.PendingBalance,
                              };
                return Json(results, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult GetPricing(string id)
        {
            id = id ?? User.Identity.GetUserCode();
            using (var packageDataContext = new PackageDataContext())
            {
                var pricing = packageDataContext.Pricings.Where(a => a.UserCode == id.ToUpper()).Select(a => new
                {
                    Channel = a.Channel1.ID,
                    a.Channel1.Name,
                    a.Channel1.Category,
                    a.Price
                }).ToList();
                if (pricing.Count == 0)
                {
                    pricing = packageDataContext.Channels.Select(a => new
                    {
                        Channel = a.ID,
                        a.Name,
                        a.Category,
                        Price = a.DefaultPrice,
                    }).ToList();
                }
                return Json(pricing, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Users = MvcApplication.ADMIN_USERS)]
        public ActionResult SavePricing(Pricing[] pricing)
        {
            using (var packageDataContext = new PackageDataContext())
            {
                packageDataContext.Pricings.DeleteAllOnSubmit(packageDataContext.Pricings.Where(a => a.UserCode == pricing[0].UserCode.ToUpper()));
                packageDataContext.Pricings.InsertAllOnSubmit(pricing);
                packageDataContext.SubmitChanges();
            }
            return Json("Success");
        }

        public ActionResult GetBalance()
        {
            return Json(User.Identity.GetBalance(), JsonRequestBehavior.AllowGet);
        }

        
        public const string USER_TYPE_LOCAL = "本地客户";
        public const string USER_TYPE_GENERAL = "普通用户";

        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Account");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}