using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System.Numerics;

namespace LordsContract
{
    public static class Periodical
    {
       

        public static byte[] SimpleDropItems(BigInteger itemId)
        {
            //  TODO check that item exists.
            //  TODO check block interval
            // TODO check that item is from Batch

            BigInteger[] occupied = new BigInteger[10] {0,0,0,0,0,0,0,0,0,0};
            int occupiedAmount = 0;

            string key;
            BigInteger checkedId = 1;
            Stronghold stronghold;
            byte[] bytes;

            for (int i = 1; i <= 10; i++, checkedId = checkedId+1)
            {
                key = GeneralContract.STRONGHOLD_PREFIX + checkedId.AsByteArray();
                bytes = Storage.Get(Storage.CurrentContext, key);

                if (bytes.Length > 1)
                {
                    stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(bytes);
                    if (stronghold.Hero > 0)
                    {
                        occupied[occupiedAmount] = checkedId;
                        occupiedAmount++;
                    }
                }
            }

            if (occupiedAmount == 0)
            {
                Runtime.Notify("All strongholds are occupied by NPC");
                return new BigInteger(1).AsByteArray();
            }

            // Check that Item has no owner
            string itemKey = GeneralContract.MANAGABLE_ITEM_PREFIX + itemId.AsByteArray();
            bytes = Storage.Get(Storage.CurrentContext, itemKey);
            if (bytes.Length < 1)
            {
                Runtime.Log("Item doesn't exist on Blockchain!");
                return new BigInteger(1).AsByteArray();
            }

            Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(bytes);
            if (item.BATCH != GeneralContract.STRONGHOLD_REWARD_BATCH)
            {
                Runtime.Log("Item is not allowed to be dropped. Only Stronghold Batch items are allowed");
                return new BigInteger(1).AsByteArray();
            }

            Runtime.Log("Before Random");

            // returned an index on list of available strongholds ids
            BigInteger random = GeneralContract.GetRandomNumber((ulong)occupiedAmount);

            //Runtime.Log("After random");
            //Storage.Put(Storage.CurrentContext, ExecutionEngine.CallingScriptHash, random);
            Runtime.Notify("Random int ", random);

            random = Helper.GetByIntIndex(occupied, occupiedAmount, random);
            //random = occupied[(int)random - 1]; // from index get stronghold id

            Runtime.Log("Random stronghold");
            //Storage.Put(Storage.CurrentContext, ExecutionEngine.CallingScriptHash, random);

            key = GeneralContract.STRONGHOLD_PREFIX + random.AsByteArray();
            bytes = Storage.Get(Storage.CurrentContext, key);
            stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(bytes);

            BigInteger lordId = stronghold.Hero;

            Runtime.Log("Lord id");
            Storage.Put(Storage.CurrentContext, ExecutionEngine.CallingScriptHash, lordId);

            string heroKey = GeneralContract.HERO_PREFIX + lordId.AsByteArray();
            Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, heroKey));

            Runtime.Log("Returned Stronghold Data");
            // Change owner of Item.
            item.OWNER = hero.OWNER;
            item.BATCH = GeneralContract.NO_BATCH;
            byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

            // Save Item
            Storage.Put(Storage.CurrentContext, itemKey, itemBytes);

            // Delete Stronghold owner. (It means "kicking out from Stronghold"). According to rule, if stronghold owner got item, he should be kicked out from stronghold
            Storage.Delete(Storage.CurrentContext, key);

            Runtime.Log("Returned 0");
            //Set Stronghold update time.
            DropData dropped = new DropData();
            dropped.Block = Blockchain.GetHeight();
            dropped.HeroId = lordId;
            dropped.ItemId = itemId;
            dropped.StrongholdId = random;

            bytes = Neo.SmartContract.Framework.Helper.Serialize(dropped);

            Runtime.Log("Returned 1");

            BigInteger incrementor = Helper.GetDropIncrementor();
            byte[] incrementorBytes = incrementor.AsByteArray();
            key = GeneralContract.STRONGHOLD_REWARD_PREFIX + incrementorBytes;

            Storage.Put(Storage.CurrentContext, key, bytes);

            Runtime.Log("Returs 2");

            incrementor = incrementor + 1;

            Runtime.Log("Returned 3");

            byte[] number = incrementor.AsByteArray();
            Runtime.Log("Converted to S");
            Storage.Put(Storage.CurrentContext, GeneralContract.DROPPED_INCREMENTOR, number);
            Runtime.Log("Incrementor");

            //Helper.SetDropIncrementor(incrementor);

            Runtime.Log("Item was Dropped successfully");
            return new BigInteger(1).AsByteArray();
        }

        /**
         * Function records item drop
         * 
         * Function drops item in every 120 blocks. Usually called by Server Side of Blocklords.
         * 
         * Has 0 argument
         */
        public static byte[] DropItems(BigInteger itemId)
        {
            // TODO check that item is in drop item list
            // Between each Item Drop as a reward should be generated atleast 120 Blocks
            return SimpleDropItems(itemId);
        }


        //------------------------------------------------------------------------------------
        //
        // City Coffers Payout
        //
        //------------------------------------------------------------------------------------
        public static byte[] CofferPayout()
        {
            // Check Witness
            if (!Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                Runtime.Log("Permission denied! Only game admin can use this function!");
                return new BigInteger(0).AsByteArray();
            }

            // TODO check Coffer payment interval

            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            BigInteger checkedId = 1;

            // City Coffer Cut.
            for (var i = 0; i < 16; i++, checkedId = checkedId + 1)
            {
                string key = GeneralContract.CITY_PREFIX + checkedId.AsByteArray();
                byte[] cityBytes = Storage.Get(Storage.CurrentContext, key);



                if (cityBytes.Length == 0)
                {
                    Runtime.Notify("City doesn't exist!");
                    continue;
                    //return new BigInteger(0).AsByteArray();
                }

                City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);

                if (city.Hero == 0)
                {
                    continue;
                }

                // Coffer is 30%

                //string heroKey = GeneralContract.HERO_PREFIX + city.Hero.AsByteArray();
                //Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

                decimal percent = city.Coffer / 100;
                decimal remainedValue = percent * 70;

                city.Coffer = remainedValue;
                cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);

                Storage.Put(Storage.CurrentContext, key, cityBytes);


                // Just send to city owners the amount of money that they need
                //foreach (var payOut in outputs)
                //{
                //    BigInteger incomingValue = payOut.Value;
                //    if (incomingValue == requiredValue && hero.OWNER == payOut.ScriptHash)
                //    {
                //        city.Coffer = city.Coffer - requiredValue;

                //        cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);

                //        Storage.Put(Storage.CurrentContext, key, cityBytes);

                //        continue;
                //    }
                //}

                Runtime.Notify("Hero doesn't receive his coffer prize!!!");
                return new BigInteger(0).AsByteArray();
            }

            BigInteger blockHeight = Blockchain.GetHeight();
            //byte[] bytes = blockHeight.AsByteArray();

            Storage.Put(Storage.CurrentContext, GeneralContract.COFFER_PAYOUT_KEY, blockHeight);

            return new BigInteger(1).AsByteArray();
        }

        //------------------------------------------------------------------------------------
        //
        // functions for:
        // ITEM DROP AS A REWARD
        //
        //------------------------------------------------------------------------------------

        //private static byte[] ComplexDropItems(BigInteger itemId)
        //{
        //    DropData dropData = (DropData)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, GeneralContract.LATEST_REWARDED_ITEM_KEY));

        //    BigInteger currentBlock = Blockchain.GetHeight();

        //    if (dropData != null && currentBlock < dropData.Block + GeneralContract.DropInterval)
        //    {
        //        Runtime.Notify("Too early to drop Items! Last Block", dropData.Block, "Current Block", currentBlock);
        //        return new BigInteger(0).AsByteArray();
        //    }

        //    BigInteger totalStrongholdBlocks = 0;
        //    BigInteger[] blockStarts = new BigInteger[10];
        //    BigInteger[] blockEnds = new BigInteger[10];

        //    string key = "";
        //    Stronghold stronghold;

        //    // There are 10 Strongholds on the map
        //    for (int i = 0; i < 10; i++)            // i + 1 = Stronghold ID
        //    {
        //        key = GeneralContract.STRONGHOLD_PREFIX + (i + 1).Serialize();

        //        byte[] strongholdBytes = Storage.Get(Storage.CurrentContext, key);

        //        if (strongholdBytes.Length == 0)
        //        {
        //            blockStarts[i] = blockEnds[i] = 0;      // Stronghold has no Owner. Skipping it
        //        }
        //        else
        //        {
        //            stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(strongholdBytes);

        //            if (i == 0)
        //                blockStarts[i] = 0;
        //            else
        //                blockStarts[i] = blockEnds[i - 1];

        //            BigInteger blocks = Blockchain.GetHeight() - stronghold.CreatedBlock;

        //            blockEnds[i] = blockStarts[i] + blocks;

        //            totalStrongholdBlocks += blocks;
        //        }


        //    }

        //    if (totalStrongholdBlocks == 0)
        //    {
        //        Runtime.Notify("There are no Stronghold owners");
        //        return new BigInteger(0).AsByteArray();
        //    }
        //    else
        //    {
        //        BigInteger randomBlock = GeneralContract.GetRandomNumber((ulong)totalStrongholdBlocks);
        //        Runtime.Notify("Returned Random Stronghold Block", randomBlock, "Define Stronghold ID");  // To Debug

        //        // Find Stronghold based on block
        //        for (var i = 0; i < 10; i++)
        //        {
        //            if (randomBlock >= blockStarts[i] && randomBlock <= blockEnds[i])    //TODOOOOOOOOOOOOOOOOOOOOOOOOOOO if random block will be equal to min or max, this logic command will return FALSE?
        //            {
        //                // Is Stronghold has an owner?
        //                key = GeneralContract.STRONGHOLD_PREFIX + (i + 1).Serialize();
        //                break;
        //            }
        //        }



        //        byte[] strongholdBytes = Storage.Get(Storage.CurrentContext, key);
        //        stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(strongholdBytes);

        //        //if (stronghold.Hero <= 0)
        //        //{
        //        //    Runtime.Notify("Stronghold is owned by NPC");
        //        //    return new BigInteger(0).AsByteArray();
        //        //}

        //        // The Stronghold was randomly selected. Now, give to owner of Stronghold the item
        //        var nextRewardItem = Storage.Get(Storage.CurrentContext, GeneralContract.NEXT_REWARD_ITEM_KEY);
        //        if (nextRewardItem.Length == 0)
        //        {
        //            Runtime.Notify("Item to reward for stronghold owning doesn't exist");
        //            return new BigInteger(0).AsByteArray();
        //        }



        //        string itemKey = GeneralContract.MANAGABLE_ITEM_PREFIX + nextRewardItem;
        //        Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, itemKey));

        //        string heroKey = GeneralContract.HERO_PREFIX + stronghold;
        //        Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, heroKey));


        //        // Change owner of Item.
        //        item.OWNER = hero.OWNER;
        //        byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

        //        // Save Item Owner Changing
        //        Storage.Put(Storage.CurrentContext, itemKey, itemBytes);

        //        // Delete Stronghold owner. (It means "kicking out from Stronghold"). According to rule, if stronghold owner got item, he should be kicked out from stronghold
        //        Storage.Delete(Storage.CurrentContext, key);

        //        // Since, latest item on batch is given, delete it and set previous added item on batch as a next item to reward.

        //        key = GeneralContract.STRONGHOLD_REWARD_PREFIX + nextRewardItem;
        //        BigInteger previousItem = new BigInteger(Storage.Get(Storage.CurrentContext, key));
        //        Storage.Delete(Storage.CurrentContext, key);
        //        Storage.Put(Storage.CurrentContext, GeneralContract.NEXT_REWARD_ITEM_KEY, previousItem); // Set Next Reward Item

        //        // Set Stronghold update time.
        //        DropData dropped = new DropData();
        //        dropped.Block = Blockchain.GetHeight();
        //        dropped.HeroId = stronghold.Hero;
        //        dropped.ItemId = nextRewardItem.AsBigInteger();
        //        dropped.StrongholdId = stronghold.ID;

        //        byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(dropped);

        //        Storage.Put(Storage.CurrentContext, GeneralContract.LATEST_REWARDED_ITEM_KEY, bytes);
        //    }

        //    Runtime.Notify("Item was Dropped successfully");
        //    return new BigInteger(1).AsByteArray();
        //}
    }
}


