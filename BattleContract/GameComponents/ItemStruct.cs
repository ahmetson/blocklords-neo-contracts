using BattleContract.Character;

namespace BattleContract.GameComponents
{
    public struct Item
    {
        public byte[] Stat;
        public byte[] MaxStat;
        public StatType statType;
        public QualityType Quality;
        public string Id;
    }
}
