
namespace BattleContract.StorageData
{
    struct HeroData
    {
        public int Index;
        public int Length;
        public HeroDataType parameterType;

        public HeroData(HeroDataType p, int i, int l)
        {
            parameterType = p;
            Index = i;
            Length = l;
        }
    }

}
