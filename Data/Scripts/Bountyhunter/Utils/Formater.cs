using Bountyhunter.Store;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bountyhunter.Utils
{
    class Formater
    {
        static Dictionary<int, string> CharWidth = new Dictionary<int, string>()
        {
            {17,  "3FKTabdeghknopqsuy£µÝàáâãäåèéêëðñòóôõöøùúûüýþÿāăąďđēĕėęěĝğġģĥħĶķńņňŉōŏőśŝşšŢŤŦũūŭůűųŶŷŸșȚЎЗКЛбдекруцяёђћўџ"},
            {21,  "ABDNOQRSÀÁÂÃÄÅÐÑÒÓÔÕÖØĂĄĎĐŃŅŇŌŎŐŔŖŘŚŜŞŠȘЅЊЖф□"},
            {19,  "#0245689CXZ¤¥ÇßĆĈĊČŹŻŽƒЁЌАБВДИЙПРСТУХЬ€"},
            {20,  "￥$&GHPUVY§ÙÚÛÜÞĀĜĞĠĢĤĦŨŪŬŮŰŲОФЦЪЯжы†‡"},
            {8, "！ !I`ijl ¡¨¯´¸ÌÍÎÏìíîïĨĩĪīĮįİıĵĺļľłˆˇ˘˙˚˛˜˝ІЇії‹›∙" },
            {16,  "？7?Jcz¢¿çćĉċčĴźżžЃЈЧавийнопсъьѓѕќ"},
            {9,  "（）：《》，。、；【】(),.1:;[]ft{}·ţťŧț"},
            {18, "+<=>E^~¬±¶ÈÉÊË×÷ĒĔĖĘĚЄЏЕНЭ−" },
            {15, "L_vx«»ĹĻĽĿŁГгзлхчҐ–•" },
            {10, "\"-rª­ºŀŕŗř" },
            {31, "WÆŒŴ—…‰" },
            {6, "'|¦ˉ‘’‚" },
            {25, "@©®мшњ" },
            {27, "mw¼ŵЮщ" },
            {14, "/ĳтэє" },
            {12, "\\°“”„" },
            {11, "*²³¹" },
            {28, "¾æœЉ" },
            {24, "%ĲЫ" },
            {26, "MМШ"},
            {29, "½Щ" },
            {23, "ю" },
            {7, "j" },
            {22, "љ" },
            {13, "ґ" },
            {30, "™" }
        };
        static Dictionary<char, int> CharWidthCache = new Dictionary<char, int>();

        public static string FormatNumber(double value)
        {
            string suffix = "";
            if (value >= 1000000)
            {
                value /= 1000000;
                suffix = "m";
            } else if (value >= 1000)
            {
                value /= 1000;
                suffix = "k";
            }
            if (value >= 100) return value.ToString("0") + suffix;
            else if (value >= 1)
            {
                bool isEven = int.Parse(value.ToString("0")) == double.Parse(value.ToString("0.0"));
                return value.ToString(isEven ? "0" : "0.0") + suffix;
            }
            else if (value >= 0.1)
            {
                bool isEven = int.Parse(value.ToString("0")) == double.Parse(value.ToString("0.00"));
                return value.ToString(isEven ? "0" : "0.00") + suffix;
            }
            else if (value >= 0.001)
            {
                bool isEven = int.Parse(value.ToString("0")) == double.Parse(value.ToString("0.000"));
                return value.ToString(isEven ? "0" : "0.000") + suffix;
            }
            else
            {
                return "<0.001";
            }
        }

        public static string FormatCurrency(double value)
        {
            if (value == 0) return "-";
            else return FormatNumber(value) + Config.Instance.CurrencyName;
        }

        public static int GetWidth(char c)
        {
            int width;
            if (!CharWidthCache.TryGetValue(c, out width))
            {
                foreach(KeyValuePair<int, string> entry in CharWidth)
                {
                    if(entry.Value.Contains(c.ToString()))
                    {
                        width = entry.Key;
                        break;
                    }
                }
                if(width == 0)
                {
                    width = 19;
                }
                CharWidthCache.Add(c, width);
            }
            return width;
        }

        public static int GetWidth(string text)
        {
            int width = 0;
            foreach(char c in text.ToCharArray())
            {
                width += GetWidth(c);
            }
            return width;
        }

        public static string PadLeft(string text, int targetWidth, char padding = ' ')
        {
            int width = GetWidth(text);
            if (width >= targetWidth) return text;
            else return new string(padding, (int)Math.Floor((float)(targetWidth - width) / GetWidth(padding))) + text;
        }

        public static string PadRight(string text, int targetWidth, char padding = ' ')
        {
            int width = GetWidth(text);
            if (width >= targetWidth) return text;
            else return text + new string(padding, (int)Math.Floor((float)(targetWidth - width) / GetWidth(padding)));
        }

        public static string PadCenter(string text, int targetWidth, char padding = ' ')
        {
            int width = GetWidth(text);
            if (width >= targetWidth) return text;
            else {
                float todo = (float)Math.Floor((float)(targetWidth - width) / GetWidth(padding));
                int left = (int)Math.Floor(todo / 2);
                int right = (int)Math.Ceiling(todo / 2);
                return new string(padding, left) + text + new string(padding, right);
            }
        }
    }
}
