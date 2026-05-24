using System;

namespace Treasures.CurrencySystem
{
    public enum CurrencyType
    {
        Gold,
        Diamond
    }

    [Serializable]
    public class CurrencyValue
    {
        public CurrencyType Type;
        public int Value;
    }
}