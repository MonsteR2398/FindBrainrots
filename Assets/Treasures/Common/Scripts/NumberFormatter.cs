using UnityEngine;

namespace ModularTreasures
{
    public enum NumberFormatMode
    {
        Raw,         // 1000000
        Separated,   // 1.000.000
        Abbreviated  // 1K, 1M, 1B
    }

    public static class NumberFormatter
    {
        public static string Format(float value, NumberFormatMode mode)
        {
            // If it's a small value with decimals, we might want to keep them, 
            // but for currency/quests we usually deal with integers.
            // However, to be safe:
            if (mode == NumberFormatMode.Abbreviated)
                return Abbreviate((long)value);
            
            return Format((long)value, mode);
        }

        public static string Format(long value, NumberFormatMode mode)
        {
            switch (mode)
            {
                case NumberFormatMode.Raw:
                    return value.ToString();
                case NumberFormatMode.Separated:
                    var nfi = new System.Globalization.NumberFormatInfo { NumberGroupSeparator = ".", NumberDecimalDigits = 0 };
                    return value.ToString("N", nfi);
                case NumberFormatMode.Abbreviated:
                    return Abbreviate(value);
                default:
                    return value.ToString();
            }
        }

        private static string Abbreviate(long value)
        {
            if (value < 1000 && value > -1000) return value.ToString();
            
            bool isNegative = value < 0;
            long absValue = System.Math.Abs(value);
            
            var nfi = new System.Globalization.NumberFormatInfo { NumberDecimalSeparator = "." };
            string result;

            if (absValue < 1000000) 
                result = (absValue / 1000f).ToString("F1", nfi).TrimEnd('0').TrimEnd('.') + "K";
            else if (absValue < 1000000000) 
                result = (absValue / 1000000f).ToString("F1", nfi).TrimEnd('0').TrimEnd('.') + "M";
            else 
                result = (absValue / 1000000000f).ToString("F1", nfi).TrimEnd('0').TrimEnd('.') + "B";

            return isNegative ? "-" + result : result;
        }
    }
}