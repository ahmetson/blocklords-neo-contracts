
using System.Numerics;

namespace BattleContract.GameComponents
{
    public class Helper
    {
        public static BigInteger StringToQualityType(string value)
        {
            if (value.Equals("1"))
            {
                return 1;// QualityType.SSR;
            }
            else if (value.Equals("2"))
            {
                return 2;// QualityType.SR;
            }
            return 3;// QualityType.R;
        }
        public static string QualityTypeToString(BigInteger qualityType)
        {
            if (qualityType == 1)
            {
                return "1";
            }
            if (qualityType == 2)
            {
                return "2";
            }
            return "3";
        }
    }
}
