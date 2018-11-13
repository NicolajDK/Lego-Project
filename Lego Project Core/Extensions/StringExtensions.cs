using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lego_Project_Core.Extensions
{
    public static class StringExtensions
    {
        public static string GetTeaser(this string body, int length, string postFix)
        {
            if (String.IsNullOrWhiteSpace(body))
                return "";

            if (body.Length <= length)
                return body;

            return body.Substring(0, length) + postFix;
        }
    }
}
