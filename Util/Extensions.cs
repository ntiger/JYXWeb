using JYXWeb.DB;
using System.Security.Claims;
using System.Security.Principal;
using System.Linq;

namespace JYXWeb.Util
{
    public static class Extensions
    {
        public static string GetUserCode(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("UserCode");
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetFirstName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FirstName");
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetLastName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("LastName");
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static double GetBalance(this IIdentity identity)
        {
            var userCode = identity.GetUserCode();
            using (var dataContext = new PackageDataContext())
            {
                return dataContext.Transactions.Where(a => a.UserCode == userCode).OrderByDescending(a => a.ID).Select(a => a.Balance).FirstOrDefault() ?? 0;
            }
        }

        public static double GetCredit(this IIdentity identity)
        {
            var userCode = identity.GetUserCode();
            using (var dataContext = new PackageDataContext())
            {
                return dataContext.Transactions.Where(a => a.UserCode == userCode).OrderByDescending(a => a.ID).Select(a => a.Credit).FirstOrDefault() ?? 0;
            }
        }

        public static string FormatBalance(this double balance)
        {
            return string.Format("{0:c}", balance);
        }
    }
}