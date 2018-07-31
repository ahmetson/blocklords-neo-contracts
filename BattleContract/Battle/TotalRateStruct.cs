
using BattleContract.Math;

namespace BattleContract.Battle
{
    public struct TotalRate {
        public Range[] Rates;  // Rates for Ids
        public int[] Ids;      // Ids with unique Increasing values

        public int Total;

        /*public TotalRate(Range idsRange)
        {
            Rates = new Range[Math.Helper.GetLength(idsRange)];
            Total = 0;
            Ids = Math.Helper.AsArray(idsRange);
        }*/

        
    }
}
