
using BattleContract.Math;

namespace BattleContract.Battle
{
    public class TotalRate {
        public Range Rate0, Rate1, Rate2, Rate3, Rate4;  // Rates for Ids
        public int[] Ids;      // Ids with unique Increasing values

        public int Total;

        public TotalRate(Range idsRange)
        {
            Rate0 = new Range();
            Rate1 = new Range();
            Rate2 = new Range();
            Rate3 = new Range();
            Rate4 = new Range();
            Total = 0;
            Ids = Math.Helper.AsArray(idsRange);
        }

        public static Range GetRateByIndex(TotalRate tr, int i)
        {
            if (i == 0) return tr.Rate0;
            if (i == 1) return tr.Rate1;
            if (i == 2) return tr.Rate2;
            if (i == 3) return tr.Rate3;
            return tr.Rate4;
        }
        public static void SetRateByIndex(TotalRate tr, int i, Range newValue)
        {
            if (i == 0) tr.Rate0 = newValue;
            if (i == 1) tr.Rate1 = newValue;
            if (i == 2) tr.Rate2 = newValue;
            if (i == 3) tr.Rate3 = newValue;
            if (i == 3) tr.Rate3 = newValue;
        }
    }
}
