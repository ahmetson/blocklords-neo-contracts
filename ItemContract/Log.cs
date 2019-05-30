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

        public static void Battle(object[] args)
        {
            if (Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                Runtime.Notify(1);
                throw new System.Exception();
            }

            Runtime.Log("Battle Init");

            // Prepare log
            BattleLog log = new BattleLog();

            log.BattleId = (byte[])args[0];
            log.BattleResult = (BigInteger)args[1]; // 0 - Attacker WON, 1 - Attacker Lose
            log.BattleType = (BigInteger)args[2];   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            log.Attacker = (byte[])args[3]; // Hero
            log.AttackerTroops = (BigInteger)args[4];
            log.AttackerRemained = (BigInteger)args[5];
            log.DefenderObject = (byte[])args[6];   // City|Stronghold|Bandit Camp ID
            log.DefenderTroops = (BigInteger)args[7];
            log.DefenderRemained = (BigInteger)args[8];

            Runtime.Log("Retreive data");

            string battleIdKey = GeneralContract.BATTLE_LOG_MAP + log.BattleId;
            byte[] battleLogBytes = Storage.Get(Storage.CurrentContext, battleIdKey);
            if (battleLogBytes.Length > 0)
            {
                Runtime.Notify(7002);
                throw new System.Exception();
            }

            Runtime.Log("Battle Log is not exist");

            // Get Hero
            string heroKey = GeneralContract.HERO_MAP + log.Attacker;
            byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
            if (heroBytes.Length <= 0)
            {
                Runtime.Notify(7003);
                throw new System.Exception();
            }

            Runtime.Log("attacker on blockchain");

            Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
            if (!Runtime.CheckWitness(hero.OWNER))
            {
                Runtime.Notify(7004);
                throw new System.Exception();
            }

            Runtime.Log("Attacker owning wallet calls");

            log.AttackerItem1 = (byte[])args[9];
            log.AttackerItem2 = (byte[])args[10];
            log.AttackerItem3 = (byte[])args[11];
            log.AttackerItem4 = (byte[])args[12];
            log.AttackerItem5 = (byte[])args[13];

            Runtime.Log("Get Items");

            CheckItemOwnership(log.AttackerItem1, log.Attacker);
            CheckItemOwnership(log.AttackerItem2, log.Attacker);
            CheckItemOwnership(log.AttackerItem3, log.Attacker);
            CheckItemOwnership(log.AttackerItem4, log.Attacker);
            CheckItemOwnership(log.AttackerItem5, log.Attacker);

            Runtime.Log("Items owned by player");

            byte[] attackerId = log.Attacker;
            BigInteger attackerNum = (BigInteger)args[3];
            if (attackerNum == 0)
            {
                Runtime.Log("Hero Number is 0");
            }

            // Get Hero of Defender
            string key;
            byte[] bytes;
            if (log.BattleType == GeneralContract.PVC)
            {
                Runtime.Log("Attack to city");
                key = GeneralContract.CITY_MAP + log.DefenderObject;
                bytes = Storage.Get(Storage.CurrentContext, key);

                if (bytes.Length <= 0)
                {
                    Runtime.Notify(7005);
                    throw new System.Exception();
                }
                else
                {
                    Runtime.Log("City Exists");
                    City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(bytes);

                    if (city.Hero > 0 && city.Hero == attackerNum)
                    {
                        Runtime.Notify(7006);
                        throw new System.Exception();
                    }
                    else
                    {
                        Runtime.Log("Hero is attacking to someones city");
                        byte[] feeBytes = Storage.Get(Storage.CurrentContext, GeneralContract.FEE_PVC);

                        BigInteger fee = feeBytes.AsBigInteger();

                        if (!GeneralContract.AttachmentExistAB(feeBytes, GeneralContract.GameOwner))
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
                            city.Hero = attackerNum;
                        }
                        else if (log.BattleResult != GeneralContract.ATTACKER_LOSE)
                        {
                            Runtime.Notify(7008);
                            throw new System.Exception();
                        }


                        key = GeneralContract.CITY_MAP + log.DefenderObject;
                        bytes = Neo.SmartContract.Framework.Helper.Serialize(city);
                        Storage.Put(Storage.CurrentContext, key, bytes);
                    }
                }

            }
            else if (log.BattleType == GeneralContract.PVP)
            {
                Runtime.Log("PVP battle");
                key = GeneralContract.STRONGHOLD_MAP + log.DefenderObject;
                bytes = Storage.Get(Storage.CurrentContext, key);

                if (bytes.Length <= 0)
                {
                    Runtime.Notify(7009);
                    throw new System.Exception();
                }
                else
                {
                    Runtime.Log("Stronghold is on blockchain");
                    if (hero.StrongholsAmount > 0)
                    {
                        Runtime.Notify(7010);
                        throw new System.Exception();
                    }

                    Stronghold stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(bytes);
                    if (stronghold.Hero > 0 && stronghold.Hero == attackerNum)
                    {
                        Runtime.Notify(7010);
                        throw new System.Exception();
                    }
                    else
                    {
                        Runtime.Log("Stronghold is not owned by player");
                        byte[] feeBytes = Storage.Get(Storage.CurrentContext, GeneralContract.FEE_PVP);
                        BigInteger fee = feeBytes.AsBigInteger();

                        if (!GeneralContract.AttachmentExist(fee, GeneralContract.GameOwner))
                        {
                            Runtime.Notify(7012);
                            throw new System.Exception();
                        }

                        Runtime.Log("Attachment is included");

                        if (log.BattleResult == GeneralContract.ATTACKER_WON)
                        {
                            // change city owner
                            stronghold.Hero = attackerNum;
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

                        Runtime.Log("Stronghold attack data prepared");

                        key = GeneralContract.STRONGHOLD_MAP + log.DefenderObject;
                        bytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);
                        Storage.Put(Storage.CurrentContext, key, bytes);

                        Runtime.Log("Stronghold data inserted");
                    }
                }
            }
            else if (log.BattleType == GeneralContract.PVE)
            {
                Runtime.Log("Bandit camp attack");
                key = GeneralContract.BANDIT_CAMP_MAP + log.DefenderObject;
                bytes = Storage.Get(Storage.CurrentContext, key);

                if (bytes.Length <= 0)
                {
                    Runtime.Notify(7014);
                    throw new System.Exception();
                }
                else
                {
                    Runtime.Log("Bandit camp on blockchain");
                    byte[] feeBytes = Storage.Get(Storage.CurrentContext, GeneralContract.FEE_PVE);
                    BigInteger fee = feeBytes.AsBigInteger();

                    if (!GeneralContract.AttachmentExist(fee, GeneralContract.GameOwner))
                    {
                        Runtime.Notify(7015);
                        throw new System.Exception();
                    }

                    if (log.BattleResult == GeneralContract.ATTACKER_WON)
                    {
                        Runtime.Log("Bandit camp attacker won");
                        UpdateItemStats(log.AttackerItem1, log.AttackerItem2, log.AttackerItem3, log.AttackerItem4, log.AttackerItem5, log.BattleId, 2);
                    }
                    else if (log.BattleResult == GeneralContract.ATTACKER_LOSE)
                    {
                        Runtime.Log("Bandit camp attacker Lose");
                        UpdateItemStats(log.AttackerItem1, log.AttackerItem2, log.AttackerItem3, log.AttackerItem4, log.AttackerItem5, log.BattleId, 1);
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

            Runtime.Log("Battle typ  specific data change finished");

            log.Time = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;

            Runtime.Log("Battle data returned");

            battleLogBytes = Neo.SmartContract.Framework.Helper.Serialize(log);
            Storage.Put(Storage.CurrentContext, battleIdKey, battleLogBytes);

            Runtime.Log("Battle Data on blockchain");

            Runtime.Notify(7000, log.BattleId, log.BattleResult,
                log.BattleType, log.Attacker, log.AttackerTroops,
                log.AttackerRemained,
                log.AttackerItem1, log.AttackerItem2, log.AttackerItem3,
                log.AttackerItem4, log.AttackerItem5, log.DefenderObject,
                log.DefenderTroops, log.DefenderRemained);
        }

        private static bool IsUpgradableItem(byte[] itemId)
        {
            byte[] bytes;
            Item item;

            string key = GeneralContract.ITEM_MAP + itemId;
            bytes = Storage.Get(Storage.CurrentContext, key);

            if (bytes.Length > 0)
            {
                item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(bytes);

                // If item reached to max level, then it's not counted as upgradable
                if (item.QUALITY == 1 && item.LEVEL >= 3) return false;
                if (item.QUALITY == 2 && item.LEVEL >= 5) return false;
                if (item.QUALITY == 3 && item.LEVEL >= 7) return false;
                if (item.QUALITY == 4 && item.LEVEL >= 9) return false;
                if (item.QUALITY == 5 && item.LEVEL >= 10) return false;

                Runtime.Log("Item is upgradable");
                return true;
            }
            Runtime.Log("Item is not Upgradable");
            return false;
        }

        private static void UpdateItemStats(byte[] id1, byte[] id2, byte[] id3, byte[] id4, byte[] id5, 
            byte[] battleId, BigInteger exp)
        {
            byte[][] upgradableItem = new byte[5][];
            BigInteger upgradableAmount = 0;

            int checkedIndex = 0;

            if (IsUpgradableItem(id1))
            {
                upgradableItem[checkedIndex] = id1; checkedIndex++;
                upgradableAmount = BigInteger.Add(upgradableAmount, 1);
            }
            if (IsUpgradableItem(id2))
            {
                upgradableItem[checkedIndex] = id2; checkedIndex++;
                upgradableAmount = BigInteger.Add(upgradableAmount, 1);
            }
            if (IsUpgradableItem(id3))
            {
                upgradableItem[checkedIndex] = id3; checkedIndex++;
                upgradableAmount = BigInteger.Add(upgradableAmount, 1);
            }
            if (IsUpgradableItem(id4))
            {
                upgradableItem[checkedIndex] = id4; checkedIndex++;
                upgradableAmount = BigInteger.Add(upgradableAmount, 1);
            }
            if (IsUpgradableItem(id5))
            {
                upgradableItem[checkedIndex] = id5; checkedIndex++;
                upgradableAmount = BigInteger.Add(upgradableAmount, 1);
            }

            if (upgradableAmount == 0)
            {
                return;
            }

            BigInteger randomUpgradableIndex =  GeneralContract.GetRandomNumber(0, (ulong)upgradableAmount);

            byte[] itemId = Helper.GetIdByIndex(upgradableItem, upgradableAmount, randomUpgradableIndex);
            if (itemId.Length <= 0)
            {
                Runtime.Log("Random generated is 0");
                return;
            }

            string key = GeneralContract.ITEM_MAP + itemId;

            Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

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
            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            Runtime.Log("Item exp increased");
            Runtime.Notify(7019, itemId, battleId, exp, item.LEVEL, item.STAT_VALUE, item.XP);
        }

        public static void CheckItemOwnership(byte[] itemId, byte[] itemOwner)
        {
            Runtime.Log("Check item");
            string itemKey = GeneralContract.ITEM_MAP + itemId;
            byte[] itemBytes = Storage.Get(Storage.CurrentContext, itemKey);
            BigInteger itemOwnerNum = itemOwner.AsBigInteger();
            Runtime.Log("Item prepared");
            if (itemBytes.Length > 0)
            {
                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes);
                if (itemOwnerNum == 0)
                    Runtime.Log("Check ownership item not exist");
                else
                    Runtime.Log("Item checked not 0");
                if (item.HERO != itemOwnerNum && item.BATCH != GeneralContract.NO_BATCH)
                {
                    Runtime.Notify(8002);
                    throw new System.Exception();
                }
            }
        }
    }


    
}


