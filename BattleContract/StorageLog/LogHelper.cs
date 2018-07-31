using BattleContract.Battle;
using BattleContract.Character;
using BattleContract.StorageData;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace BattleContract.StorageLog
{
    public static class Helper
    {
        public static void AddIncreasedItem(BattleLog log, string id, string stat, int increasing)
        {
            log.increasingsNumber++;
            string value = id + StringAndArray.Helper.GetStringByDigit(increasing) + StringAndArray.Helper.GetZeroPrefixedString(stat, 4);
            log.Increasings[log.increasingsNumber - 1] = value;
        }

        public static string GetIncreasingsAsLogParameter(BattleLog battleLog)
        {
            string parameter = "";

            for(int i=0; i<battleLog.increasingsNumber; i++)
            {
                parameter = parameter + battleLog.Increasings[0];
            }
            if (battleLog.increasingsNumber==ItemDataHelper.MaxItems) return parameter;

            // Add Empty Parameters
            int parameterLength = 13 + 1 + 4;   // ID (13) INCREASING (1) + STAT (4)
            for(int i= battleLog.increasingsNumber; i< ItemDataHelper.MaxItems; i++)
            {
                parameter = parameter + StringAndArray.Helper.GetZeroPrefixedString("", parameterLength);
            }

            return parameter;
        }

        public static string GetRemainedTroops (BattleLog battleLog, bool isMy)
        {
            BigInteger remainedTroops = battleLog.EnemyRemainedTroops;//.ToByteArray();
            if (isMy)
                remainedTroops = battleLog.MyRemainedTroops;//.ToByteArray();
            return StringAndArray.Helper.GetZeroPrefixedString(remainedTroops.ToByteArray().AsString(), 4);
        }
        public static string GetIncreasingsNumber(BattleLog battleLog)
        {
            return StringAndArray.Helper.GetStringByDigit((int)battleLog.increasingsNumber);
        }

        public static string GetStorageParameters(BattleLog battleLog, Hero my, Hero enemy)
        {
            Runtime.Log("Get my remained troops");
            byte[] enemyRemainBytes = battleLog.EnemyRemainedTroops.AsByteArray();
            string enemyRemains = enemyRemainBytes.AsString();
            Runtime.Log(">Enemy Remained Troops" + enemyRemains);
            byte[] myRemainBytes = battleLog.MyRemainedTroops.AsByteArray();
            string myRemains = myRemainBytes.AsString();
            Runtime.Log(">My remained Troops" + myRemains);

            Runtime.Log("My remained troops defined ");
            string myRemainedTroops = GetZeroPrefixedString(myRemains, 4);
            Runtime.Log("My remained troops are defined");
            Runtime.Notify(enemyRemains, myRemains);
            string enemyRemainedTroops = GetZeroPrefixedString(enemyRemains, 4);
            Runtime.Log("Enemies remained troops are defined");
            string increasingNumber = StringAndArray.Helper.GetStringByDigit(battleLog.increasingsNumber);
            Runtime.Log("Both remained parameters calculated");
            Runtime.Notify("Put on log the following parameters: ", battleLog.battleResult, 
                battleLog.battleType, my.Id, myRemainedTroops, enemy.Id, enemyRemainedTroops, increasingNumber);
            return battleLog.battleResult + battleLog.battleType + my.Id + myRemainedTroops + enemy.Id + enemyRemainedTroops +
                /*increasingNumber + GetIncreasingsAsLogParameter(battleLog) + */my.Owner.AsString() + enemy.Owner.AsString();
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

        /*public static void AddToLog(BattleLog battleLog, BattleResult battleResult)
        {
            battleLog.battleResult = StringAndArray.Helper.GetStringByDigit((int)battleResult);
        }
        public static void AddToLog(BattleLog battleLog, BigInteger troops, bool isMy)
        {
            if (isMy)
                battleLog.MyRemainedTroops = troops;
            battleLog.EnemyRemainedTroops = troops;
        }
        public static void AddToLog(BattleLog battleLog, BigInteger myTroops, BigInteger enemyTroops)
        {
            AddToLog(battleLog, myTroops, true);
            AddToLog(battleLog, myTroops, false);
        }
        public static void AddToLog(BattleLog battleLog,  BattleResult battleResult, BigInteger myTroops, BigInteger enemyTroops)
        {
            AddToLog(battleLog, battleResult);
            AddToLog(battleLog, myTroops, enemyTroops);
        }*/

        // LOG RECORDS ON STORAGE IS:
        //  BATTLE ID (13)  <= KEY
        //
        //  BATTLE_RESULT (1)   HERO #1 ID (13) HERO #1 TROOPS (4)  HERO #1 REMAINED TROOPS (4),
        //  HERO #2 ID (13) HERO #2 TROOPS (4)  HERO #2 REMAINED TROOPS (4), STAT INCREASED ITEMS NO (1)
        //  ( ITEM ID (13)  INCREASED VALUE (1) STAT BEFORE INCREASING (4) ) x5, BATTLE TYPE (1)
        //  HERO #1 OWNER
        //  HERO #2 OWNER
        public static void LogBattleResult(BattleLog battleLog, Hero my, Hero enemy)
        {
            Runtime.Log("Log the result");
            byte[] enemyRemainBytes = battleLog.EnemyRemainedTroops.AsByteArray();
            string enemyRemains = enemyRemainBytes.AsString();
            Runtime.Log(">Enemy Remained Troops" + enemyRemains);
            byte[] myRemainBytes = battleLog.MyRemainedTroops.AsByteArray();
            string myRemains = myRemainBytes.AsString();
            
            Runtime.Log(">Battle Result " + battleLog.battleResult);
            Runtime.Log(">Battle Type " + battleLog.battleType);

            string parameters = battleLog.battleResult + battleLog.battleType;
            Runtime.Log("Parameter #1: " + parameters);

            Runtime.Log(">My ID " + my.Id);
            parameters = parameters + my.Id;
            Runtime.Log("Parameter #2: " + parameters);

            Runtime.Log(">My remained Troops " + myRemains);
            parameters = parameters + myRemains;
            Runtime.Log("Parameter #3: " + parameters);

            Runtime.Log(">Enemy ID " + enemy.Id);
            parameters = parameters + enemy.Id;
            Runtime.Log("Parameter #4: " + parameters);

            Runtime.Log(">Enemy remained Troops " + enemyRemains);
            parameters = parameters + enemyRemains;
            Runtime.Log("Parameter #5: " + parameters);
            /*increasingNumber + GetIncreasingsAsLogParameter(battleLog) + */

            string myOwner = my.Owner.AsString();
            string enemyOwner = enemy.Owner.AsString();

            Runtime.Log(">My Owner " + myOwner);
            parameters = parameters + myOwner;
            Runtime.Log("Parameter #6: " + parameters);

            Runtime.Log(">Enemy Owner " + enemyOwner);
            parameters = parameters + enemyOwner;
            Runtime.Log("Parameter #7: " + parameters);

            Storage.Put(Storage.CurrentContext, battleLog.BattleId, parameters);
            Runtime.Log("Battle result has been recorded on Blockchain!!!");
        }
    }
}
