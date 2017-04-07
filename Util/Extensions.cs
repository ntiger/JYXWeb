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
                return dataContext.Transactions.Where(a => a.UserCode == userCode).OrderByDescending(a => a.ID)
                    .Select(a => a.Balance).FirstOrDefault();
            }
        }

        public static double GetUnitPrice(this IIdentity identity, int channelID)
        {
            var userCode = identity.GetUserCode();
            using (var dataContext = new PackageDataContext())
            {
                var channel = dataContext.Channels.Where(a => a.ID == channelID).Single();
                var existingPricing = channel.Pricings.Where(a => a.UserCode == userCode).SingleOrDefault();
                if (existingPricing == null)
                {
                    return channel.DefaultPrice.Value;
                }
                return existingPricing.Price.Value;
            }
        }

        public static string FormatBalance(this double balance)
        {
            return string.Format("{0:c}", balance);
        }
    }
}