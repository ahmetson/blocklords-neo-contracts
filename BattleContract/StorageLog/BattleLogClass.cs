using BattleContract.Battle;
using System.Numerics;

namespace BattleContract.StorageLog
{
    public class BattleLog
    {
        public string[] Increasings;
        public int increasingsNumber;
        public string battleResult;
        public string BattleId;
        public BigInteger MyRemainedTroops;
        public BigInteger EnemyRemainedTroops;
        public string battleType;


        public BattleLog(string battleId)
        {
            BattleId = battleId;
            battleType = StringAndArray.Helper.GetStringByDigit(1);
            EnemyRemainedTroops = 0;
            MyRemainedTroops = 0;
            Increasings = new string[0];
            increasingsNumber = 0;
            battleResult = StringAndArray.Helper.GetStringByDigit((int)BattleResult.BOTH_LOSE);
        }
    }
}
