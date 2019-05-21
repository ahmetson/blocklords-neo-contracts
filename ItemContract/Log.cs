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
            if (Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                Runtime.Log("GAME_OWNER_CAN_NOT_PLAY_GAME");
                throw new System.Exception();
            }

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

            string battleIdKey = GeneralContract.BATTLE_LOG_MAP + log.BattleId.AsByteArray();
            byte[] battleLogBytes = Storage.Get(Storage.CurrentContext, battleIdKey);
            if (battleLogBytes.Length > 0)
            {
                Runtime.Log("BATTLE_WITH_ID_MUST_NOT_BE_ON_BLOCKCHAIN");
                throw new System.Exception();
            }

            // Get Hero
            string heroKey = GeneralContract.HERO_MAP + log.Attacker.AsByteArray();
            byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
            if (heroBytes.Length <= 0)
            {
                Runtime.Log("ATTACKING_HERO_MUST_BE_ON_BLOCKCHAIN");
                throw new System.Exception();
            }
            Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
            if (!Runtime.CheckWitness(hero.OWNER))
            {
                Runtime.Log("ATTACKING_HERO_MUST_BE_INVOLVED_IN_BATTLE_BY_OWNER");
                throw new System.Exception();
            }

            BigInteger[] attackerItems = new BigInteger[5]
            {
                (BigInteger)args[9],
                (BigInteger)args[10],
                (BigInteger)args[11],
                (BigInteger)args[12],
                (BigInteger)args[13]
            };

            CheckItemOwnership(attackerItems[0], log.Attacker);
            CheckItemOwnership(attackerItems[1], log.Attacker);
            CheckItemOwnership(attackerItems[2], log.Attacker);
            CheckItemOwnership(attackerItems[3], log.Attacker);
            CheckItemOwnership(attackerItems[4], log.Attacker);


            // Set default defender as a NPC
            //log.Defender = 0;           // NPC data
            //log.DefenderItem1 = 0;
            //log.DefenderItem2 = 0;
            //log.DefenderItem3 = 0;
            //log.DefenderItem4 = 0;
            //log.DefenderItem5 = 0;

            // Get Hero of Defender
            string key;
            byte[] bytes;
            if (log.BattleType == GeneralContract.PVC)
            {
                key = GeneralContract.CITY_MAP + log.DefenderObject.AsByteArray();
                bytes = Storage.Get(Storage.CurrentContext, key);

                if (bytes.Length <= 0)
                {
                    Runtime.Log("ATTACKED_CITY_MUST_BE_ON_BLOCKCHAIN");
                    throw new System.Exception();
                }
                else
                {
                    City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(bytes);
                    if (city.Hero == log.Attacker)
                    {
                        Runtime.Log("ATTACKER_MUST_BE_NON_CITY_LORD");
                        throw new System.Exception();
                    }
                    else
                    {
                        byte[] feeBytes = Storage.Get(Storage.CurrentContext, GeneralContract.FEE_PVC);
                        BigInteger fee = feeBytes.AsBigInteger();

                        if (!GeneralContract.AttachmentExist(fee, GeneralContract.GameOwner))
                        {
                            Runtime.Log("PVC_FEE_MUST_BE_INCLUDED");
                            throw new System.Exception();
                        }

                        // Increase city coffer
                        byte[] pvcCofferPercentsBytes = Storage.Get(Storage.CurrentContext, GeneralContract.PERCENTS_PVC_COFFER);
                        BigInteger pvcCofferPercents = pvcCofferPercentsBytes.AsBigInteger();
                        BigInteger percent = BigInteger.Divide(fee, 100);
                        BigInteger pvcCoffer = BigInteger.Multiply(pvcCofferPercents, percent);

                        city.Coffer = BigInteger.Add(city.Coffer, pvcCoffer);

                        if (log.BattleResult == GeneralContract.ATTACKER_WON)
                        {
                            // change city owner
                            city.Hero = log.Attacker;
                        }
                        else if (log.BattleResult != GeneralContract.ATTACKER_LOSE)
                        {
                            Runtime.Log("BATTLE_RESULT_MUST_BE_VALID");
                            throw new System.Exception();
                        }

                        bytes = Neo.SmartContract.Framework.Helper.Serialize(city);
                        Storage.Put(Storage.CurrentContext, key, bytes);
                    }
                }

            }
            else if (log.BattleType == GeneralContract.PVP)
            {
                key = GeneralContract.STRONGHOLD_MAP + log.DefenderObject.AsByteArray();
                bytes = Storage.Get(Storage.CurrentContext, key);

                if (bytes.Length <= 0)
                {
                    Runtime.Log("ATTACKED_STRONGHOLD_MUST_BE_ON_BLOCKCHAIN");
                    throw new System.Exception();
                }
                else
                {
                    if (hero.StrongholsAmount > 0)
                    {
                        Runtime.Log("ATTACKER_MUST_BE_NON_STRONGHOLD_LORD");
                        throw new System.Exception();
                    }

                    Stronghold stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(bytes);
                    if (stronghold.Hero == log.Attacker)
                    {
                        Runtime.Log("ATTACKER_MUST_BE_NON_STRONGHOLD_LORD");
                        throw new System.Exception();
                    }
                    else
                    {
                        byte[] feeBytes = Storage.Get(Storage.CurrentContext, GeneralContract.FEE_PVP);
                        BigInteger fee = feeBytes.AsBigInteger();

                        if (!GeneralContract.AttachmentExist(fee, GeneralContract.GameOwner))
                        {
                            Runtime.Log("PVP_FEE_MUST_BE_INCLUDED");
                            throw new System.Exception();
                        }

                        if (log.BattleResult == GeneralContract.ATTACKER_WON)
                        {
                            // change city owner
                            stronghold.Hero = log.Attacker;
                            stronghold.CreatedBlock = Blockchain.GetHeight();

                            hero.StrongholsAmount = 1;

                            heroBytes = Neo.SmartContract.Framework.Helper.Serialize(hero);
                            Storage.Put(Storage.CurrentContext, heroKey, heroBytes);
                        }
                        else if (log.BattleResult != GeneralContract.ATTACKER_LOSE)
                        {
                            Runtime.Log("BATTLE_RESULT_MUST_BE_VALID");
                            throw new System.Exception();
                        }

                        bytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);
                        Storage.Put(Storage.CurrentContext, key, bytes);
                    }
                }
            }
            else if (log.BattleType == GeneralContract.PVE)
            {
                key = GeneralContract.BANDIT_CAMP_MAP + log.DefenderObject.AsByteArray();
                bytes = Storage.Get(Storage.CurrentContext, key);

                if (bytes.Length <= 0)
                {
                    Runtime.Log("ATTACKED_BANDIT_CAMP_MUST_BE_ON_BLOCKCHAIN");
                    throw new System.Exception();
                }
                else
                {
                    byte[] feeBytes = Storage.Get(Storage.CurrentContext, GeneralContract.FEE_PVE);
                    BigInteger fee = feeBytes.AsBigInteger();

                    if (!GeneralContract.AttachmentExist(fee, GeneralContract.GameOwner))
                    {
                        Runtime.Log("PVE_FEE_MUST_BE_INCLUDED");
                        throw new System.Exception();
                    }

                    if (log.BattleResult == GeneralContract.ATTACKER_WON)
                    {
                        UpdateItemStats(attackerItems, log.BattleId, 2);
                    }
                    else if (log.BattleResult == GeneralContract.ATTACKER_LOSE)
                    {
                        UpdateItemStats(attackerItems, log.BattleId, 1);
                    }
                    else
                    {
                        Runtime.Log("BATTLE_RESULT_MUST_BE_VALID");
                        throw new System.Exception();
                    }

                    
                }
            }
            else
            {
                Runtime.Log("BATTLE_TYPE_MUST_BE_VALID");
                throw new System.Exception();
            }

            log.Time = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;

            battleLogBytes = Neo.SmartContract.Framework.Helper.Serialize(log);
            Storage.Put(Storage.CurrentContext, battleIdKey, battleLogBytes);

            Runtime.Notify("Battle was logged on Blockchain");
            return new BigInteger(1).AsByteArray();
        }
        
        private static void UpdateItemStats(BigInteger[] ids, BigInteger battleId, BigInteger exp)
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

                    // If item reached to max level, then it's not counted as upgradable
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
            BigInteger index =  GeneralContract.GetRandomNumber(0, (ulong)upgradableAmount);

            Runtime.Log("Before getting random id");
            BigInteger itemId = Helper.GetByIntIndex(upgradable, upgradableAmount, index);

            Runtime.Log("Before preparing key");

            key = GeneralContract.ITEM_MAP + itemId.AsByteArray();

            //Storage.Put(Storage.CurrentContext, index.AsByteArray(), upgradableAmount);

            Runtime.Log("Before getting data");
            item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            Runtime.Log("Before Increase of XP");
            // Increase XP that represents on how many items the Item was involved
            item.XP = BigInteger.Add(item.XP, exp);

            Runtime.Log("Before Increase of Stats");
            // Increase Level
            if (item.LEVEL == 0 && item.XP >= 2 ||
                    item.LEVEL == 1 && item.XP >= 6 ||
                    item.LEVEL == 2 && item.XP >= 20 ||
                    item.LEVEL == 3 && item.XP >= 48 ||
                    item.LEVEL == 4 && item.XP >= 92 ||
                    item.LEVEL == 5 && item.XP >= 152 ||
                    item.LEVEL == 6 && item.XP >= 228 ||
                    item.LEVEL == 7 && item.XP >= 318 ||
                    item.LEVEL == 8 && item.XP >= 434 ||
                    item.LEVEL == 9 && item.XP >= 580
            ) {
                item.LEVEL = item.LEVEL + 1;
                item.STAT_VALUE = item.STAT_VALUE + 1;
            }

            Runtime.Log("Before putting item on blockchain");
            // Put back On Storage the Item with increased values
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            Runtime.Log("returning data");

        }

        public static void CheckItemOwnership(BigInteger itemId, BigInteger itemOwner)
        {
            string itemKey = GeneralContract.ITEM_MAP + itemId.AsByteArray();
            byte[] itemBytes = Storage.Get(Storage.CurrentContext, itemKey);
            if (itemBytes.Length <= 0)
            {
                Runtime.Log("ITEM_MUST_BE_ON_BLOCKCHAIN");
                throw new System.Exception();
            }
            Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes);
            if (item.HERO != itemOwner || item.BATCH != GeneralContract.NO_BATCH)
            {
                Runtime.Log("ITEM_MUST_BE_OWNED_BY_SOMEONE_AND_MUST_BE_OUT_OF_BATCH");
                throw new System.Exception();
            }
        }
    }


    
}


