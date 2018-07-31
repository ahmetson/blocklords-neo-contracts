namespace BattleContract.Battle
{
    public struct ItemIncreasing {
        public int Id;
        public int Rate;
        public int Increasing;

        public ItemIncreasing(int i, int r, int c)
        {
            Id = i;
            Rate = r;
            Increasing = c;
        }
    }
}
