using BattleContract.Battle;
using System.Numerics;

namespace BattleContract.StorageLog
{
    public struct BattleLog
    {
        public string[] Increasings;
        public int increasingsNumber;
        public BattleResult battleResult;
        public string BattleId;
        public BigInteger MyRemainedTroops;
        public BigInteger EnemyRemainedTroops;
        public BattleType battleType;


        /*public BattleLog(string battleId)
        {
            BattleId = battleId;
            battleType = BattleType.PVP;
            EnemyRemainedTroops = 0;
            MyRemainedTroops = 0;
            Increasings = new string[0];
            increasingsNumber = 0;
            battleResult = BattleResult.BOTH_LOSE;
        }*/
    }
}
