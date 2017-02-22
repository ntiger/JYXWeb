using System;
using System.Linq;

namespace JYXWeb.Util
{
    public class AppUtil
    {
        public static string GetUserCode(int size = 6)
        {
            string input = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            Random random = new Random();
            var chars = Enumerable.Range(0, size)
                                   .Select(x => input[random.Next(0, input.Length)]);
            return new string(chars.ToArray());
        }

        public static bool IsAdmin(string userCode)
        {
            if (userCode == "AAAAAA" || userCode == "BBBBBB")
            {
                return true;
            }
            return false;
        }
    }
}