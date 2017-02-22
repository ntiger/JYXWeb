using System.Security.Claims;
using System.Security.Principal;

namespace JYXWeb.Util
{
    public static class IdentityExtensions
    {
        public static string GetUserCode(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("UserCode");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetFirstName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FirstName");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }

        public static string GetLastName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("LastName");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}