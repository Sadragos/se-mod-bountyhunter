using Bountyhunter.Store;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bountyhunter.Utils
{
    class Formater
    {
        public static string FormatNumber(double value)
        {
            // TODO even numbers
            if (value >= 10000000) return (value / 1000000).ToString("0.0") + "m";
            if (value >= 1000000) return (value / 1000000).ToString("0.00") + "m";
            if (value >= 100000) return (value / 1000).ToString("0") + "k";
            if (value >= 10000) return (value / 1000).ToString("0.0") + "k";
            if (value >= 1000) return (value / 1000).ToString("0.00") + "k";
            if (value >= 10) return (value).ToString("0");
            if (value >= 1) return (value).ToString("0.0");
            if (value >= 0.1) return (value).ToString("0.00");
            if (value >= 0.01) return (value).ToString("0.000");
            return "< 0.01";
        }

        public static string FormatCurrency(double value)
        {
            if (value == 0) return "-";
            else return FormatNumber(value) + Config.Instance.CurrencyName;
        }
    }
}
