using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hectic7
{
    public static class BetterHelper
    {
        public static TEnum EGet<TEnum>(this Dictionary<string, int> dict)
        {
            var key = typeof(TEnum).FullName;
            return (TEnum)Enum.ToObject(typeof(TEnum), dict[key]);
        }

        public static void ESet<TEnum>(this Dictionary<string, int> dict, TEnum enumVal) where TEnum : struct
        {
            var val = (int)(ValueType)enumVal;
            var key = typeof(TEnum).FullName;
            if (dict.ContainsKey(key))
                dict[key] = (int)val;
            else
                dict.Add(key, val);
        }

        public static Dictionary<string, int> Clone(this Dictionary<string, int> dict, Action<Dictionary<string, int>> onCloned = null)
        {
            var clone = new Dictionary<string, int>(dict);
            if (onCloned != null)
                onCloned(clone);
            return clone;
        }

        public static string EGetTip<TEnum>(this Dictionary<string, int> dict)
        {
            var key = typeof(TEnum).FullName;
            if (!dict.ContainsKey(key))
                return "???";
            return ((TEnum)Enum.ToObject(typeof(TEnum), dict[key])).ToString().Replace("Bullet", "");
        }

        public static bool IsValid(this Dictionary<string, int> dict)
        {
            return BetterPattern.PatternProperties.All(p => dict.ContainsKey(p.FullName));
        }
    }
}
