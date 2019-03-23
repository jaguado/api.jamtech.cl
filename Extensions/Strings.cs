using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JAMTech.Extensions
{
    public static class Strings
    {
        public static int? OnlyNumbers(this string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var onlyNumbers = Regex.Replace(value, "[^0-9]", "").Trim();
            if(!string.IsNullOrEmpty(onlyNumbers))
                return int.Parse(onlyNumbers);
            return null;
        }
        public static string OnlyLastWord(this string value)
        {
            return value.Trim().Split(' ').Last().Trim();
        }
    }
}
