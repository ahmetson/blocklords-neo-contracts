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
            log.DefenderObject = (BigInteger)args[6];   // City|Stronghold|Bandit Camp ID
            log.DefenderTroops = (BigInteger)args[7];
            log.DefenderRemained = (BigInteger)args[8];
            log.AttackerOwner = ExecutionEngine.CallingScriptHash;

            // Get Hero
            string heroKey = GeneralContract.HERO_MAP + log.Attacker.AsByteArray();
            byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
            if (heroBytes.Length == 0)
            {
                Runtime.Log("Attacker doesn't exist");
                return new BigInteger(0).AsByteArray();
            }
            Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
            Runtime.Log("Hero is");
            byte[] serialized = Neo.SmartContract.Framework.Helper.Serialize(hero.OWNER);
            Storage.Put(Storage.CurrentContext, serialized, ExecutionEngine.CallingScriptHash);
            Runtime.Notify(hero);
            //if (ExecutionEngine.CallingScriptHash.AsBigInteger() != serialized.AsBigInteger())
            //{
            //    Runtime.Log("Only owner of Attacker can log the battle result!");
            //    return new BigInteger(0).AsByteArray();
            //}

            BigInteger[] attackerItems = new BigInteger[5]
            {
                (BigInteger)args[9],
                (BigInteger)args[10],
                (BigInteger)args[11],
                (BigInteger)args[12],
                (BigInteger)args[13]
            };


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
            if (log.BattleType == GeneralContract.PVC)
            {
                // Get City
                // If city doesn't exist, return error
                // If city doesn't have an owner, write nothing
                // If city has an owner, check that it is not the attacker, otherwise return error
                // Write hero id, hero equopments
                key = GeneralContract.CITY_MAP + log.DefenderObject.AsByteArray();
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
                    heroKey = GeneralContract.HERO_MAP + city.Hero.AsByteArray();
                    heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                    // We sure, that hero data exists, so we will not check it for existence
                    Hero defenderHero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);

                    log.Defender = city.Hero;           // Real Hero
                    //log.DefenderItem1 = defenderHero.Equipments[0];
                    //log.DefenderItem2 = defenderHero.Equipments[1];
                    //log.DefenderItem3 = defenderHero.Equipments[2];
                    //log.DefenderItem4 = defenderHero.Equipments[3];
                    //log.DefenderItem5 = defenderHero.Equipments[4];
                }
            }
            else if (log.BattleType == GeneralContract.PVP)
            {
                // Get City
                // If city doesn't exist, return error
                // If city doesn't have an owner, write nothing
                // If city has an owner, check that it is not the attacker, otherwise return error
                // Write hero id, hero equopments
                key = GeneralContract.STRONGHOLD_MAP + log.DefenderObject.AsByteArray();
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
                    heroKey = GeneralContract.HERO_MAP + stronghold.Hero.AsByteArray();
                    heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                    // We sure, that hero data exists, so we will not check it for existence
                    Hero defenderHero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);

                    log.Defender = stronghold.Hero;           // Real Hero
                    //log.DefenderItem1 = defenderHero.Equipments[0];
                    //log.DefenderItem2 = defenderHero.Equipments[1];
                    //log.DefenderItem3 = defenderHero.Equipments[2];
                    //log.DefenderItem4 = defenderHero.Equipments[3];
                    //log.DefenderItem5 = defenderHero.Equipments[4];
                }
            }
            else if (log.BattleType == GeneralContract.PVE)
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
            //if (log.BattleType == GeneralContract.CityType && ! GeneralContract.IsTransactionOutputExist(GeneralContract.cityAttackFee))
            //        {
            //            Runtime.Notify("The Battle Fee has not been included.");
            //            return new BigInteger(0).AsByteArray();
            //        } else if (log.BattleType == GeneralContract.StrongholdType && ! GeneralContract.IsTransactionOutputExist(GeneralContract.strongholdAttackFee))
            //        {
            //            Runtime.Notify("The Battle Fee has not been included.");
            //            return new BigInteger(0).AsByteArray();
            //        } else if (log.BattleType == GeneralContract.BanditCampType && ! GeneralContract.IsTransactionOutputExist(GeneralContract.banditCampAttackFee))
            //        {
            //            Runtime.Notify("The Battle Fee has not been included.");
            //            return new BigInteger(0).AsByteArray();
            //        }


            // Log 
            key = GeneralContract.BATTLE_LOG_PREFIX + log.TX;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(log);
            Storage.Put(Storage.CurrentContext, key, bytes);

            // Apply Battle Result
            // If battle type is city attack, then change owner of the city
            // If battle type is stronghold attack, then change owner of the stronghold
            // If battle type is bandit camp attack, update item.

            if (log.BattleType == GeneralContract.PVC)
            {
                key = GeneralContract.CITY_MAP + log.DefenderObject.AsByteArray();
                City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

                // Change City Lord
                if (log.BattleResult == 1)  // Attacker Won?
                {

                    city.Hero = log.Attacker;

                }

                // Increase City Coffer
                decimal attackFee = GeneralContract.cityAttackFee / 2;
                city.Coffer = city.Coffer + attackFee;

                // Save City Information
                byte[] cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
                Storage.Put(Storage.CurrentContext, key, cityBytes);
            } else if (log.BattleType == GeneralContract.PVP)
            {
                // Change Stronghold Occupier
                key = GeneralContract.STRONGHOLD_MAP + log.DefenderObject.AsByteArray();
                if (log.BattleResult == 1) // Attacker Won?
                {
                    Stronghold stronghold = new Stronghold();
                    stronghold.CreatedBlock = Blockchain.GetHeight();
                    stronghold.ID = log.DefenderObject;
                    stronghold.Hero = log.Attacker;

                    bytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);

                    Storage.Put(Storage.CurrentContext, key, bytes);
                }
            } else if (log.BattleType == GeneralContract.PVE)
            {
                UpdateItemStats(attackerItems, log.BattleId);
            }

            Runtime.Notify("Battle was logged on Blockchain");
            return new BigInteger(1).AsByteArray();
        }

        public static byte[] StrongholdLeave(object[] args)
        {

            Runtime.Notify("Stronghold Leaving Initiated");

            // Change Stronghold Lord
            string key = GeneralContract.STRONGHOLD_MAP + ((BigInteger)args[0]).AsByteArray();

            Stronghold stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            if (stronghold.Hero == 0)
            {
                Runtime.Notify("Stronghold is already owned by NPC.");
                return new BigInteger(0).AsByteArray();
            }

            string heroKey = GeneralContract.HERO_MAP + stronghold.Hero.AsByteArray();
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

        private static void UpdateItemStats(BigInteger[] id, BigInteger battleId)
        {
            string key;
            // Algorithm of this function:
            // 1. Define a list with upgradable items.
            //  Upgradable items are not empty, and didn't reach to max level
            // 2. Pick Random Upgradable Item
            // 3. Increase Item Exp by 2.
            // 4. Increase Level Value too
            // 5. Increase Stat Value too.
            // 6. Log
            //

            BigInteger[] upgradable = new BigInteger[5] { 0, 0, 0, 0, 0 };
            int upgradableAmount = 0;

            BigInteger checkedId = 1;
            byte[] bytes;
            Item item;

            for (int i = 1; i <= 5; i++, checkedId = checkedId + 1)
            {
                key = GeneralContract.ITEM_MAP + checkedId.AsByteArray();
                bytes = Storage.Get(Storage.CurrentContext, key);

                if (bytes.Length > 1)
                {
                    item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(bytes);

                    if (item.QUALITY == 1 && item.LEVEL == 3) continue;
                    if (item.QUALITY == 2 && item.LEVEL == 5) continue;
                    if (item.QUALITY == 3 && item.LEVEL == 7) continue;
                    if (item.QUALITY == 4 && item.LEVEL == 9) continue;
                    if (item.QUALITY == 5 && item.LEVEL == 10) continue;

                    Runtime.Log("Item is upgradable");
                    upgradable[upgradableAmount] = checkedId;
                    upgradableAmount++;
                }
            }

            if (upgradableAmount == 0)
            {
                Runtime.Notify("There are no items to upgrade");
                return;
            }

            Runtime.Log("Before Generation");
            BigInteger index =  GeneralContract.GetRandomNumber((ulong)upgradableAmount);

            Runtime.Log("Before getting random id");
            BigInteger itemId = Helper.GetByIntIndex(upgradable, upgradableAmount, index);

            Runtime.Log("Before preparing key");

            key = GeneralContract.ITEM_MAP + itemId.AsByteArray();

            //Storage.Put(Storage.CurrentContext, index.AsByteArray(), upgradableAmount);

            Runtime.Log("Before getting data");
            item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            Runtime.Log("Before Increase of XP");
            // Increase XP that represents on how many items the Item was involved
            item.XP = item.XP + 2;

            Runtime.Log("Before Increase of Stats");
            // Increase Level
            if (item.LEVEL == 0 && item.XP == 2 ||
                    item.LEVEL == 1 && item.XP == 6 ||
                    item.LEVEL == 2 && item.XP == 20 ||
                    item.LEVEL == 3 && item.XP == 48 ||
                    item.LEVEL == 4 && item.XP == 92 ||
                    item.LEVEL == 5 && item.XP == 152 ||
                    item.LEVEL == 6 && item.XP == 228 ||
                    item.LEVEL == 7 && item.XP == 318 ||
                    item.LEVEL == 8 && item.XP == 434 ||
                    item.LEVEL == 9 && item.XP == 580
            ) {
                item.LEVEL = item.LEVEL + 1;
                item.STAT_VALUE = item.STAT_VALUE + 1;
            }

            Runtime.Log("Before putting item on blockchain");
            // Put back On Storage the Item with increased values
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            Runtime.Log("Before putting drop data on blockchain");
            bytes = itemId.AsByteArray();

            string keyUpdate = GeneralContract.UPDATED_STAT_PREFIX + battleId.AsByteArray();

            Storage.Put(Storage.CurrentContext, keyUpdate, bytes);

            Runtime.Log("Before returning data");

        }
    }


    
}


