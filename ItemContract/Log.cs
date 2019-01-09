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

        public static byte[] Battle(object[] args)
        {
            // Prepare log
            BattleLog log = new BattleLog();

            log.BattleId = (BigInteger)args[0];
            log.BattleResult = (BigInteger)args[1]; // 0 - Attacker WON, 1 - Attacker Lose
            log.BattleType = (BigInteger)args[2];   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            log.Attacker = (BigInteger)args[3]; // Hero
            log.AttackerTroops = (BigInteger)args[4];
            log.AttackerRemained = (BigInteger)args[5];
            log.DefenderObject = (BigInteger)args[5];   // City|Stronghold|Bandit Camp ID
            log.DefenderTroops = (BigInteger)args[6];
            log.DefenderRemained = (BigInteger)args[7];
            log.AttackerOwner = ExecutionEngine.CallingScriptHash;

            // Get Hero
            string heroKey = GeneralContract.HERO_PREFIX + log.Attacker.AsByteArray();
            byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
            if (heroBytes.Length == 0)
            {
                Runtime.Log("Attacker doesn't exist");
                return new BigInteger(0).AsByteArray();
            }
            Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
            if (!Runtime.CheckWitness(hero.OWNER))
            {
                Runtime.Log("Only owner of Attacker can log the battle result!");
                return new BigInteger(0).AsByteArray();
            }

            log.AttackerItem1 = hero.Equipments[0];
            log.AttackerItem2 = hero.Equipments[1];
            log.AttackerItem3 = hero.Equipments[2];
            log.AttackerItem4 = hero.Equipments[3];
            log.AttackerItem5 = hero.Equipments[4];

            // Set default defender as a NPC
            log.Defender = 0;           // NPC data
            log.DefenderItem1 = 0;
            log.DefenderItem2 = 0;
            log.DefenderItem3 = 0;
            log.DefenderItem4 = 0;
            log.DefenderItem5 = 0;

            // Get Hero of Defender
            string key;
            byte[] bytes;
            if (log.BattleType == GeneralContract.CityType)
            {
                // Get City
                // If city doesn't exist, return error
                // If city doesn't have an owner, write nothing
                // If city has an owner, check that it is not the attacker, otherwise return error
                // Write hero id, hero equopments
                key = GeneralContract.CITY_PREFIX + log.DefenderObject.AsByteArray();
                bytes = Storage.Get(Storage.CurrentContext, key);
                if (bytes.Length == 0)
                {
                    Runtime.Log("Defending City doesn't exist");
                    return new BigInteger(0).AsByteArray();
                }
                City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(bytes);
                if (city.Hero == log.Attacker)
                {
                    Runtime.Log("Lord can not attack his own City");
                    return new BigInteger(0).AsByteArray();
                }
                if (city.Hero != 0)
                {
                    heroKey = GeneralContract.HERO_PREFIX + city.Hero.AsByteArray();
                    heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                    // We sure, that hero data exists, so we will not check it for existence
                    Hero defenderHero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);

                    log.Defender = city.Hero;           // Real Hero
                    log.DefenderItem1 = defenderHero.Equipments[0];
                    log.DefenderItem2 = defenderHero.Equipments[1];
                    log.DefenderItem3 = defenderHero.Equipments[2];
                    log.DefenderItem4 = defenderHero.Equipments[3];
                    log.DefenderItem5 = defenderHero.Equipments[4];
                }
            }
            else if (log.BattleType == GeneralContract.StrongholdType)
            {
                // Get City
                // If city doesn't exist, return error
                // If city doesn't have an owner, write nothing
                // If city has an owner, check that it is not the attacker, otherwise return error
                // Write hero id, hero equopments
                key = GeneralContract.STRONGHOLD_PREFIX + log.DefenderObject.AsByteArray();
                bytes = Storage.Get(Storage.CurrentContext, key);
                if (bytes.Length == 0)
                {
                    Runtime.Log("Defending Stronghold doesn't exist");
                    return new BigInteger(0).AsByteArray();
                }
                Stronghold stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(bytes);
                if (stronghold.Hero == log.Attacker)
                {
                    Runtime.Log("Lord can not attack his own Stronghold");
                    return new BigInteger(0).AsByteArray();
                }
                if (stronghold.Hero != 0)
                {
                    heroKey = GeneralContract.HERO_PREFIX + stronghold.Hero.AsByteArray();
                    heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                    // We sure, that hero data exists, so we will not check it for existence
                    Hero defenderHero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);

                    log.Defender = stronghold.Hero;           // Real Hero
                    log.DefenderItem1 = defenderHero.Equipments[0];
                    log.DefenderItem2 = defenderHero.Equipments[1];
                    log.DefenderItem3 = defenderHero.Equipments[2];
                    log.DefenderItem4 = defenderHero.Equipments[3];
                    log.DefenderItem5 = defenderHero.Equipments[4];
                }
            }
            else if (log.BattleType == GeneralContract.BanditCampType)
            {
                // Get Bandir Camp
                // For now we have only 10 bandit camps
                if (log.DefenderObject > 10 || log.DefenderObject < 1)
                {
                    Runtime.Log("Bandit Camp doesn't exist");
                    return new BigInteger(0).AsByteArray();
                }
            }

            log.Time = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;
            log.TX = ((Transaction)ExecutionEngine.ScriptContainer).Hash;

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
                    if (log.BattleType == GeneralContract.CityType && output.Value == GeneralContract.cityAttackFee)
                    {
                        received = true;
                        break;
                    } else if (log.BattleType == GeneralContract.StrongholdType && output.Value == GeneralContract.strongholdAttackFee)
                    {
                        received = true;
                        break;
                    } else if (log.BattleType == GeneralContract.BanditCampType && output.Value == GeneralContract.banditCampAttackFee)
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

            // Log 
            key = GeneralContract.BATTLE_LOG_PREFIX + log.TX;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(log);
            Storage.Put(Storage.CurrentContext, key, bytes);

            // Apply Battle Result
            // If battle type is city attack, then change owner of the city
            // If battle type is stronghold attack, then change owner of the stronghold
            // If battle type is bandit camp attack, update item.

            if (log.BattleType == GeneralContract.CityType)
            {
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
            } else if (log.BattleType == GeneralContract.StrongholdType)
            {
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
            } else if (log.BattleType == GeneralContract.BanditCampType)
            {
                BigInteger[] ids = new BigInteger[5]
                {
                    log.AttackerItem1,
                    log.AttackerItem2,
                    log.AttackerItem3,
                    log.AttackerItem4,
                    log.AttackerItem5
                };
                BigInteger[] stats = UpdateItemStats(ids);
            }

            Runtime.Notify("Battle was logged on Blockchain");
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


