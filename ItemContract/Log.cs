using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System.Numerics;

namespace LordsContract
{
    public static class Log
    {
        //------------------------------------------------------------------------------------
        //
        // functions for:
        // BATTLE LOG
        //
        //------------------------------------------------------------------------------------

        /**
         * Function records Battle result: Attack on City. 
         * 
         * Attacker of City invokes this function.
         * 
         * Has
         * Battle ID
         * Attacker ID
         * Defender ID
         * 
         * Has 20 arguments
         * @Battle ID (BigInteger)                  - Unique ID of Battle
         * @Battle Result (BigInteger)              - 0 means Attacker Won, 1 means Attacker Lose
         * @Attacker (BigInteger)                   - ID of Hero that initialized battle
         * @Attacker Owner (byte[])                 - Wallet Address of Hero's Owner
         * @Attacker Troops (BigInteger)                   - Amount of troops that were involved in the battle
         * @Attacker Remained Troops (BigInteger)          - Amount of troops that remained after battle
         * @Attacker Equipped Item #1 (BigInteger)         - Item that was equipped by Hero during Battle
         * @Attacker Equipped Item #2 (BigInteger)
         * @Attacker Equipped Item #3 (BigInteger)
         * @Attacker Equipped Item #4 (BigInteger)
         * @Attacker Equipped Item #5 (BigInteger)
         * 
         * @Defender (BigInteger)                           - City or Stronghold owning Hero's ID or NPC id.
         * @Defender Owner (byte[])                         - If Battle Initiator attacked City or Stronghold, then the wallet address of City or Stronghold owner
         * @Defender Troops (BigInteger)                    - Amount of troops that were involved in the battle
         * @Defender Remained Troops (BigInteger)           - Amount of troops that remained after battle
         * @Defender Equipped Item #1 (BigInteger)          - Item that was equipped by Hero during Battle
         * @Defender Equipped Item #2 (BigInteger)
         * @Defender Equipped Item #3 (BigInteger)
         * @Defender Equipped Item #4 (BigInteger)
         * @Defender Equipped Item #5 (BigInteger)
         * 
         * @Defender's Object (BigInteger)                  - Is It NPC, CITY or STRONGHOLD that was attacked by Battle Initiator
         */
        public static byte[] CityAttack(object[] args)
        {
            // Check incoming fee
            bool received = false;
            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            foreach (var output in outputs)
            {
                // Game Developers got their fee?
                if (output.ScriptHash.AsBigInteger() == GeneralContract.GameOwner.AsBigInteger())
                {
                    Runtime.Notify("Game Owner received ", output.Value);
                    if (output.Value == GeneralContract.cityAttackFee)
                    {
                        received = true;
                        break;
                    }
                }
            }

            if (!received)
            {
                Runtime.Notify("The Battle Fee doesn't included.");
                return new BigInteger(0).AsByteArray();
            }

            // Prepare log
            BattleLog log = new BattleLog();

            log.BattleId = (BigInteger)args[0];
            log.BattleResult = (BigInteger)args[1]; // 0 - Attacker WON, 1 - Attacker Lose
            log.BattleType = 1;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            log.Attacker = (BigInteger)args[2]; // Hero
            log.AttackerOwner = (byte[])args[3];    // Player Address
            log.AttackerTroops = (BigInteger)args[4];
            log.AttackerRemained = (BigInteger)args[5];
            log.AttackerItem1 = (BigInteger)args[6];    // Equipped Items that were involved
            log.AttackerItem2 = (BigInteger)args[7];
            log.AttackerItem3 = (BigInteger)args[8];
            log.AttackerItem4 = (BigInteger)args[9];
            log.AttackerItem5 = (BigInteger)args[10];
            log.DefenderObject = (BigInteger)args[20];   // City|Stronghold|NPC ID

            log.Defender = (BigInteger)args[11];
            log.DefenderOwner = (byte[])args[12];
            log.DefenderTroops = (BigInteger)args[13];
            log.DefenderRemained = (BigInteger)args[14];
            log.DefenderItem1 = (BigInteger)args[15];
            log.DefenderItem2 = (BigInteger)args[16];
            log.DefenderItem3 = (BigInteger)args[17];
            log.DefenderItem4 = (BigInteger)args[18];
            log.DefenderItem5 = (BigInteger)args[19];

            log.Time = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;
            log.TX = ((Transaction)ExecutionEngine.ScriptContainer).Hash;

            // Verify Signature
            BigInteger[] integerArgs = new BigInteger[20];
            integerArgs[0] = log.BattleId;
            integerArgs[1] = log.BattleResult; // 0 - Attacker WON, 1 - Attacker Lose
            integerArgs[2] = log.BattleType;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            integerArgs[3] = log.Attacker; // Hero
            integerArgs[4] = log.AttackerTroops;
            integerArgs[5] = log.AttackerRemained;
            integerArgs[6] = log.AttackerItem1;    // Equipped Items that were involved
            integerArgs[7] = log.AttackerItem2;
            integerArgs[8] = log.AttackerItem3;
            integerArgs[9] = log.AttackerItem4;
            integerArgs[10] = log.AttackerItem5;

            integerArgs[11] = log.Defender;
            integerArgs[12] = log.DefenderTroops;
            integerArgs[13] = log.DefenderRemained;
            integerArgs[14] = log.DefenderItem1;
            integerArgs[15] = log.DefenderItem2;
            integerArgs[16] = log.DefenderItem3;
            integerArgs[17] = log.DefenderItem4;
            integerArgs[18] = log.DefenderItem5;

            integerArgs[19] = log.DefenderObject;   // City|Stronghold|NPC ID


            // Log 
            string key = GeneralContract.BATTLE_LOG_PREFIX + log.TX;
            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(log);
            Storage.Put(Storage.CurrentContext, key, bytes);

            key = GeneralContract.CITY_PREFIX + log.DefenderObject.AsByteArray();
            City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            // Change City Lord
            if (log.BattleResult == 1)  // Attacker Won?
            {

                city.Hero = log.Attacker;

            }

            // Increase City Coffer
            BigInteger attackFee = new BigInteger(GeneralContract.cityAttackFee) / 2;
            city.Coffer = city.Coffer + attackFee;

            // Save City Information
            byte[] cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
            Storage.Put(Storage.CurrentContext, key, cityBytes);

            Runtime.Notify("City Attack was logged on Blockchain");
            return new BigInteger(1).AsByteArray();
        }

        public static byte[] StrongholdAttack(object[] args)
        {
            // Check incoming fee
            bool received = false;
            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            foreach (var output in outputs)
            {
                // Game Developers got their fee?
                if (output.ScriptHash.AsBigInteger() == GeneralContract.GameOwner.AsBigInteger())
                {
                    Runtime.Notify("Game Owner received ", output.Value);
                    if (output.Value == GeneralContract.strongholdAttackFee)
                    {
                        received = true;
                        break;
                    }
                }
            }

            if (!received)
            {
                Runtime.Notify("The Battle Fee doesn't included.");
                return new BigInteger(0).AsByteArray();
            }

            // Prepare log
            BattleLog log = new BattleLog();

            log.BattleId = (BigInteger)args[0];
            log.BattleResult = (BigInteger)args[1]; // 0 - Attacker WON, 1 - Attacker Lose
            log.BattleType = 2;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            log.Attacker = (BigInteger)args[2]; // Hero
            log.AttackerOwner = (byte[])args[3];    // Player Address
            log.AttackerTroops = (BigInteger)args[4];
            log.AttackerRemained = (BigInteger)args[5];
            log.AttackerItem1 = (BigInteger)args[6];    // Equipped Items that were involved
            log.AttackerItem2 = (BigInteger)args[7];
            log.AttackerItem3 = (BigInteger)args[8];
            log.AttackerItem4 = (BigInteger)args[9];
            log.AttackerItem5 = (BigInteger)args[10];
            log.DefenderObject = (BigInteger)args[20];   // City|Stronghold|NPC ID

            log.Defender = (BigInteger)args[11];
            log.DefenderOwner = (byte[])args[12];
            log.DefenderTroops = (BigInteger)args[13];
            log.DefenderRemained = (BigInteger)args[14];
            log.DefenderItem1 = (BigInteger)args[15];
            log.DefenderItem2 = (BigInteger)args[16];
            log.DefenderItem3 = (BigInteger)args[17];
            log.DefenderItem4 = (BigInteger)args[18];
            log.DefenderItem5 = (BigInteger)args[19];

            log.Time = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;
            log.TX = ((Transaction)ExecutionEngine.ScriptContainer).Hash;

            // Verify Signature
            BigInteger[] integerArgs = new BigInteger[20];
            integerArgs[0] = log.BattleId;
            integerArgs[1] = log.BattleResult; // 0 - Attacker WON, 1 - Attacker Lose
            integerArgs[2] = log.BattleType;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            integerArgs[3] = log.Attacker; // Hero
            integerArgs[4] = log.AttackerTroops;
            integerArgs[5] = log.AttackerRemained;
            integerArgs[6] = log.AttackerItem1;    // Equipped Items that were involved
            integerArgs[7] = log.AttackerItem2;
            integerArgs[8] = log.AttackerItem3;
            integerArgs[9] = log.AttackerItem4;
            integerArgs[10] = log.AttackerItem5;

            integerArgs[11] = log.Defender;
            integerArgs[12] = log.DefenderTroops;
            integerArgs[13] = log.DefenderRemained;
            integerArgs[14] = log.DefenderItem1;
            integerArgs[15] = log.DefenderItem2;
            integerArgs[16] = log.DefenderItem3;
            integerArgs[17] = log.DefenderItem4;
            integerArgs[18] = log.DefenderItem5;

            integerArgs[19] = log.DefenderObject;   // City|Stronghold|NPC ID


            // Log 
            string key = GeneralContract.BATTLE_LOG_PREFIX + log.TX;

            //item.Seller = Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash;

            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(log);

            Storage.Put(Storage.CurrentContext, key, bytes);

            // Change Stronghold Occupier
            key = GeneralContract.STRONGHOLD_PREFIX + log.DefenderObject.AsByteArray();
            if (log.BattleResult == 1) // Attacker Won?
            {
                Stronghold stronghold = new Stronghold();
                stronghold.CreatedBlock = Blockchain.GetHeight();
                stronghold.ID = log.DefenderObject;
                stronghold.Hero = log.Attacker;

                bytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);

                Storage.Put(Storage.CurrentContext, key, bytes);
            }

            Runtime.Notify("Stronghold Attack was logged on Blockchain");
            return new BigInteger(1).AsByteArray();
        }

        public static byte[] StrongholdLeave(object[] args)
        {

            Runtime.Notify("Stronghold Leaving Initiated");

            // Change Stronghold Lord
            string key = GeneralContract.STRONGHOLD_PREFIX + ((BigInteger)args[0]).AsByteArray();

            Stronghold stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            if (stronghold.Hero == 0)
            {
                Runtime.Notify("Stronghold is already owned by NPC.");
                return new BigInteger(0).AsByteArray();
            }

            string heroKey = GeneralContract.HERO_PREFIX + stronghold.Hero.AsByteArray();
            Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, heroKey));

            Runtime.Log("Check who calls this method!");
            if (Runtime.CheckWitness(hero.OWNER))
            {
                Runtime.Notify("Only Stronghold Owner can initialize this method");
                return new BigInteger(0).AsByteArray();
            }

            // Remove Stronghold Owner Information
            stronghold.CreatedBlock = Blockchain.GetHeight();
            stronghold.Hero = 0;

            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);

            Storage.Put(Storage.CurrentContext, key, bytes);

            Runtime.Notify("Stronghold Leaving was logged on Blockchain");
            return new BigInteger(1).AsByteArray();
        }

        public static byte[] BanditCampAttack(object[] args)
        {
            // Check incoming fee
            bool received = false;
            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            foreach (var output in outputs)
            {
                // Game Developers got their fee?
                if (output.ScriptHash.AsBigInteger() == GeneralContract.GameOwner.AsBigInteger())
                {
                    Runtime.Notify("Game Owner received ", output.Value);
                    if (output.Value == GeneralContract.banditCampAttackFee)
                    {
                        received = true;
                        break;
                    }
                }
            }

            if (!received)
            {
                Runtime.Notify("The Battle Fee doesn't included.");
                return new BigInteger(0).AsByteArray();
            }

            // Prepare log
            BattleLog log = new BattleLog();

            log.BattleId = (BigInteger)args[0];
            log.BattleResult = (BigInteger)args[1]; // 0 - Attacker WON, 1 - Attacker Lose
            log.BattleType = 3;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            log.Attacker = (BigInteger)args[2]; // Hero
            log.AttackerOwner = (byte[])args[3];    // Player Address
            log.AttackerTroops = (BigInteger)args[4];
            log.AttackerRemained = (BigInteger)args[5];
            log.AttackerItem1 = (BigInteger)args[6];    // Equipped Items that were involved
            log.AttackerItem2 = (BigInteger)args[7];
            log.AttackerItem3 = (BigInteger)args[8];
            log.AttackerItem4 = (BigInteger)args[9];
            log.AttackerItem5 = (BigInteger)args[10];
            log.DefenderObject = (BigInteger)args[11];   // City|Stronghold|NPC ID

            // Verify Signature
            BigInteger[] integerArgs = new BigInteger[20];
            integerArgs[0] = log.BattleId;
            integerArgs[1] = log.BattleResult; // 0 - Attacker WON, 1 - Attacker Lose
            integerArgs[2] = log.BattleType;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            integerArgs[3] = log.Attacker; // Hero
            integerArgs[4] = log.AttackerTroops;
            integerArgs[5] = log.AttackerRemained;
            integerArgs[6] = log.AttackerItem1;    // Equipped Items that were involved
            integerArgs[7] = log.AttackerItem2;
            integerArgs[8] = log.AttackerItem3;
            integerArgs[9] = log.AttackerItem4;
            integerArgs[10] = log.AttackerItem5;

            integerArgs[11] = 0;
            integerArgs[12] = log.DefenderTroops;
            integerArgs[13] = log.DefenderRemained;
            integerArgs[14] = 0;
            integerArgs[15] = 0;
            integerArgs[16] = 0;
            integerArgs[17] = 0;
            integerArgs[18] = 0;

            integerArgs[19] = log.DefenderObject;   // City|Stronghold|NPC ID

            // No need to record NPC data!!!

            log.Time = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;
            log.TX = ((Transaction)ExecutionEngine.ScriptContainer).Hash;

            BigInteger[] ids = new BigInteger[5]
                {
                    log.AttackerItem1,
                    log.AttackerItem2,
                    log.AttackerItem3,
                    log.AttackerItem4,
                    log.AttackerItem5
                };
            BigInteger[] stats = UpdateItemStats(ids);

            // Instead we use Defender Items List to records Hero's Item Update Values
            log.DefenderItem1 = stats[0];
            log.DefenderItem2 = stats[1];
            log.DefenderItem3 = stats[2];
            log.DefenderItem4 = stats[3];
            log.DefenderItem5 = stats[4];

            string key = GeneralContract.BATTLE_LOG_PREFIX + log.TX;

            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(log);

            Storage.Put(Storage.CurrentContext, key, bytes);


            Runtime.Notify("Bandit Camp Attack was logged on Blockchain");
            return new BigInteger(1).AsByteArray();
        }


        private static BigInteger[] UpdateItemStats(BigInteger[] ids)
        {
            Runtime.Notify("Init Item Stat Update");

            string key = "";

            BigInteger[] updateValues = new BigInteger[5] { 0, 0, 0, 0, 0 };

            for (var i = 0; i < 5; i++)
            {
                // Get Item Data
                key = GeneralContract.ITEM_PREFIX + ids[i].AsByteArray();
                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

                // Increase XP that represents on how many items the Item was involved
                item.XP = item.XP + 1;

                // Increase Level
                if (item.QUALITY == 1 && item.LEVEL == 3 ||
                    item.QUALITY == 2 && item.LEVEL == 5 ||
                    item.QUALITY == 3 && item.LEVEL == 7 ||
                    item.QUALITY == 4 && item.LEVEL == 9 ||
                    item.QUALITY == 5 && item.LEVEL == 10)
                {
                    Runtime.Notify("The Item reached max possible level. So do not update it", ids[i]);
                    updateValues[i] = 0;
                    continue;
                }

                if (item.LEVEL == 1 && item.XP >= 4 ||
                    item.LEVEL == 2 && item.XP >= 14 ||
                    item.LEVEL == 3 && item.XP >= 34 ||
                    item.LEVEL == 4 && item.XP >= 74 ||
                    item.LEVEL == 5 && item.XP >= 144 ||
                    item.LEVEL == 6 && item.XP >= 254 ||
                    item.LEVEL == 7 && item.XP >= 404 ||
                    item.LEVEL == 8 && item.XP >= 604 ||
                    item.LEVEL == 9 && item.XP >= 904)
                {
                    item.LEVEL = item.LEVEL + 1;
                }

                // Increase Stat based Quality
                if (item.QUALITY == 1)
                {
                    updateValues[i] = GeneralContract.GetRandomNumber(3);                   // Item with Quality I, can increase its Stat Value between 1 - 3
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                else if (item.QUALITY == 2)
                {
                    updateValues[i] = GeneralContract.GetRandomNumber(3) + 3;               // Item with Quality II, can increase its Stat Value between 4 - 6
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                else if (item.QUALITY == 3)
                {
                    updateValues[i] = GeneralContract.GetRandomNumber(3) + 6;               // Item with Quality III, can increase its Stat Value between 7 - 9
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                else if (item.QUALITY == 4)
                {
                    updateValues[i] = GeneralContract.GetRandomNumber(3) + 9;               // Item with Quality IV, can increase its Stat Value between 10 - 12
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                else if (item.QUALITY == 5)
                {
                    updateValues[i] = GeneralContract.GetRandomNumber(3) + 12;              // Item with Quality V, can increase its Stat Value between 13 - 15
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }

                // Put back On Storage the Item with increased values
                byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
                Storage.Put(Storage.CurrentContext, key, bytes);
            }

            return updateValues;
        }
    }


    
}


