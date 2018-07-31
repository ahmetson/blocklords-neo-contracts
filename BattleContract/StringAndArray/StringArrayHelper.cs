
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace BattleContract.StringAndArray
{
    public class Helper
    {

        public static string GetStringByDigit(int i)
        {
            if (i == 0) return "0";
            if (i == 1) return "1";
            if (i == 2) return "2";
            if (i == 3) return "3";
            if (i == 4) return "4";
            if (i == 5) return "5";
            if (i == 6) return "6";
            if (i == 7) return "7";
            if (i == 8) return "8";
            return "9";
        }
        public static string GetStringByDigit(BigInteger i)
        {
            if (i == 0) return "0";
            if (i == 1) return "1";
            if (i == 2) return "2";
            if (i == 3) return "3";
            if (i == 4) return "4";
            if (i == 5) return "5";
            if (i == 6) return "6";
            if (i == 7) return "7";
            if (i == 8) return "8";
            return "9";
        }
        public static string GetZeroPrefixedString(string toPrefix, int length)
        {
            Runtime.Notify(toPrefix, length);
            if (toPrefix.Length.Equals(length))
            {
                return toPrefix;
            }

            string str = "";
            int prefixesNumber = length - toPrefix.Length;

            // Set Zero Prefixes
            for (int i = 0; i < prefixesNumber; i++)
            {
                str = str + "0";
            }

            // Set String Value after Prefixes
            str = str + toPrefix;

            return str;
        }

        public static bool IsArrayValueExist(int[] a, int v)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] == v)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
