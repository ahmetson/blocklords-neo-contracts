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
                Runtime.Notify(1);
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
                Runtime.Notify(7002);
                throw new System.Exception();
            }

            // Get Hero
            string heroKey = GeneralContract.HERO_MAP + log.Attacker.AsByteArray();
            byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
            if (heroBytes.Length <= 0)
            {
                Runtime.Notify(7003);
                throw new System.Exception();
            }
            Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
            if (!Runtime.CheckWitness(hero.OWNER))
            {
                Runtime.Notify(7004);
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

            // Get Hero of Defender
            string key;
            byte[] bytes;
            if (log.BattleType == GeneralContract.PVC)
            {
                key = GeneralContract.CITY_MAP + log.DefenderObject.AsByteArray();
                bytes = Storage.Get(Storage.CurrentContext, key);

                if (bytes.Length <= 0)
                {
                    Runtime.Notify(7005);
                    throw new System.Exception();
                }
                else
                {
                    City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(bytes);
                    if (city.Hero == log.Attacker)
                    {
                        Runtime.Notify(7006);
                        throw new System.Exception();
                    }
                    else
                    {
                        byte[] feeBytes = Storage.Get(Storage.CurrentContext, GeneralContract.FEE_PVC);
                        BigInteger fee = feeBytes.AsBigInteger();

                        if (!GeneralContract.AttachmentExist(fee, GeneralContract.GameOwner))
                        {
                            Runtime.Notify(7007);
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
                            Runtime.Notify(7008);
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
                    Runtime.Notify(7009);
                    throw new System.Exception();
                }
                else
                {
                    if (hero.StrongholsAmount > 0)
                    {
                        Runtime.Notify(7010);
                        throw new System.Exception();
                    }

                    Stronghold stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(bytes);
                    if (stronghold.Hero == log.Attacker)
                    {
                        Runtime.Notify(7010);
                        throw new System.Exception();
                    }
                    else
                    {
                        byte[] feeBytes = Storage.Get(Storage.CurrentContext, GeneralContract.FEE_PVP);
                        BigInteger fee = feeBytes.AsBigInteger();

                        if (!GeneralContract.AttachmentExist(fee, GeneralContract.GameOwner))
                        {
                            Runtime.Notify(7012);
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
                            Runtime.Notify(7013);
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
                    Runtime.Notify(7014);
                    throw new System.Exception();
                }
                else
                {
                    byte[] feeBytes = Storage.Get(Storage.CurrentContext, GeneralContract.FEE_PVE);
                    BigInteger fee = feeBytes.AsBigInteger();

                    if (!GeneralContract.AttachmentExist(fee, GeneralContract.GameOwner))
                    {
                        Runtime.Notify(7015);
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
                        Runtime.Notify(7016);
                        throw new System.Exception();
                    }

                    
                }
            }
            else
            {
                Runtime.Notify(7017);
                throw new System.Exception();
            }

            log.Time = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;

            battleLogBytes = Neo.SmartContract.Framework.Helper.Serialize(log);
            Storage.Put(Storage.CurrentContext, battleIdKey, battleLogBytes);

            Runtime.Notify(7000, log.BattleId, log.BattleResult,
                log.BattleType, log.Attacker, log.AttackerTroops,
                log.AttackerRemained,
                log.AttackerItem1, log.AttackerItem2, log.AttackerItem3,
                log.AttackerItem4, log.AttackerItem5, log.DefenderObject,
                log.DefenderTroops, log.DefenderRemained);
            return new BigInteger(1).AsByteArray();
        }
        
        private static void UpdateItemStats(BigInteger[] ids, BigInteger battleId, BigInteger exp)
        {
            string key;

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

                    upgradable[upgradableAmount] = checkedId;
                    upgradableAmount++;
                }
            }

            if (upgradableAmount == 0)
            {
                return;
            }

            BigInteger index =  GeneralContract.GetRandomNumber(0, (ulong)upgradableAmount);

            BigInteger itemId = Helper.GetByIntIndex(upgradable, upgradableAmount, index);

            key = GeneralContract.ITEM_MAP + itemId.AsByteArray();

            item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            // Increase XP that represents on how many items the Item was involved
            item.XP = BigInteger.Add(item.XP, exp);

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

            // Put back On Storage the Item with increased values
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            Runtime.Notify(7019, itemId, battleId, exp);
        }

        public static void CheckItemOwnership(BigInteger itemId, BigInteger itemOwner)
        {
            string itemKey = GeneralContract.ITEM_MAP + itemId.AsByteArray();
            byte[] itemBytes = Storage.Get(Storage.CurrentContext, itemKey);
            if (itemBytes.Length <= 0)
            {
                Runtime.Notify(1005);
                throw new System.Exception();
            }
            Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes);
            if (item.HERO != itemOwner || item.BATCH != GeneralContract.NO_BATCH)
            {
                Runtime.Notify(8002);
                throw new System.Exception();
            }
        }
    }


    
}


