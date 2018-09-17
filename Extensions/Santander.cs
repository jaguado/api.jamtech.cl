using System;

namespace JAMTech.Extensions
{
    public static class Santander
    {
        public static string ToImporte(this double value)
        {
            var result = value.ToString().PadLeft(12, '0');
            var sign = value < 0 ? "-" : "+";
            return $"{sign}{result}00";
        }
        public static string ToImporte(this int value)
        {
            var result = value.ToString().PadLeft(12, '0');
            var sign = value < 0 ? "-" : "+";
            return $"{sign}{result}00";
        }
        public static string ToCleanRut(this string value)
        {
            return value.Replace(".", "").Replace("-", "").Replace(" ", "");
        }
    }
}
