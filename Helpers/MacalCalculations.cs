using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JAMTech.Helpers
{
    public static class MacalCalculations
    {
        public static double GetRealPrice(double price, double fiscalValue = 0)
        {
            if (fiscalValue < price)
                return price + GetComission(price) + (price * 0.015) + 75000;
            else
            {
                return price + GetComission(price) + (fiscalValue * 0.015) + 75000;
            }
        }

        public static double GetPrice(double realPrice, double fiscalValue = 0)
        {
            if (fiscalValue < realPrice)
                return realPrice - GetComission(realPrice) - (realPrice * 0.015) - 75000;
            else
            {
                return realPrice - GetComission(realPrice) - (fiscalValue * 0.015) - 75000;
            }
        }

        private static double GetComission(double price)
        {
            return price * 0.125;
        }
    }
}
