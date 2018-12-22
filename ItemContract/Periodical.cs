using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

namespace LordsContract
{
    public static class Periodical
    {
        //------------------------------------------------------------------------------------
        //
        // functions for:
        // ITEM DROP AS A REWARD
        //
        //------------------------------------------------------------------------------------

        public static byte[] DropItems()
        {
            // Between each Item Drop as a reward should be generated atleast 120 Blocks
            DropData dropData = (DropData)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, GeneralContract.LATEST_REWARDED_ITEM_KEY));

            BigInteger currentBlock = Blockchain.GetHeight();

            if (dropData != null && currentBlock < dropData.Block + GeneralContract.DropInterval)
            {
                Runtime.Notify("Too early to drop Items! Last Block", dropData.Block, "Current Block", currentBlock);
                return new BigInteger(0).AsByteArray();
            }

            BigInteger totalStrongholdBlocks = 0;
            BigInteger[] blockStarts = new BigInteger[10];
            BigInteger[] blockEnds = new BigInteger[10];

            string key = "";
            Stronghold stronghold;

            // There are 10 Strongholds on the map
            for (int i = 0; i < 10; i++)            // i + 1 = Stronghold ID
            {
                key = GeneralContract.STRONGHOLD_PREFIX + (i + 1).Serialize();

                byte[] strongholdBytes = Storage.Get(Storage.CurrentContext, key);

                if (strongholdBytes.Length == 0)
                {
                    blockStarts[i] = blockEnds[i] = 0;      // Stronghold has no Owner. Skipping it
                }
                else
                {
                    stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(strongholdBytes);

                    if (i == 0)
                        blockStarts[i] = 0;
                    else
                        blockStarts[i] = blockEnds[i - 1];

                    BigInteger blocks = Blockchain.GetHeight() - stronghold.CreatedBlock;

                    blockEnds[i] = blockStarts[i] + blocks;

                    totalStrongholdBlocks += blocks;
                }


            }

            if (totalStrongholdBlocks == 0)
            {
                Runtime.Notify("There are no Stronghold owners");
                return new BigInteger(0).AsByteArray();
            }
            else
            {
                BigInteger randomBlock = GeneralContract.GetRandomNumber((ulong)totalStrongholdBlocks);
                Runtime.Notify("Returned Random Stronghold Block", randomBlock, "Define Stronghold ID");  // To Debug

                // Find Stronghold based on block
                for (var i = 0; i < 10; i++)
                {
                    if (randomBlock >= blockStarts[i] && randomBlock <= blockEnds[i])    //TODOOOOOOOOOOOOOOOOOOOOOOOOOOO if random block will be equal to min or max, this logic command will return FALSE?
                    {
                        // Is Stronghold has an owner?
                        key = GeneralContract.STRONGHOLD_PREFIX + (i + 1).Serialize();
                        break;
                    }
                }



                byte[] strongholdBytes = Storage.Get(Storage.CurrentContext, key);
                stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(strongholdBytes);

                //if (stronghold.Hero <= 0)
                //{
                //    Runtime.Notify("Stronghold is owned by NPC");
                //    return new BigInteger(0).AsByteArray();
                //}

                // The Stronghold was randomly selected. Now, give to owner of Stronghold the item
                var nextRewardItem = Storage.Get(Storage.CurrentContext, GeneralContract.NEXT_REWARD_ITEM_KEY);
                if (nextRewardItem.Length == 0)
                {
                    Runtime.Notify("Item to reward for stronghold owning doesn't exist");
                    return new BigInteger(0).AsByteArray();
                }



                string itemKey = GeneralContract.ITEM_PREFIX + nextRewardItem;
                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, itemKey));

                string heroKey = GeneralContract.HERO_PREFIX + stronghold;
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, heroKey));


                // Change owner of Item.
                item.OWNER = hero.OWNER;
                byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

                // Save Item Owner Changing
                Storage.Put(Storage.CurrentContext, itemKey, itemBytes);

                // Delete Stronghold owner. (It means "kicking out from Stronghold"). According to rule, if stronghold owner got item, he should be kicked out from stronghold
                Storage.Delete(Storage.CurrentContext, key);

                // Since, latest item on batch is given, delete it and set previous added item on batch as a next item to reward.

                key = GeneralContract.STRONGHOLD_REWARD_ITEM_KEY_PREFIX + nextRewardItem;
                BigInteger previousItem = new BigInteger(Storage.Get(Storage.CurrentContext, key));
                Storage.Delete(Storage.CurrentContext, key);
                Storage.Put(Storage.CurrentContext, GeneralContract.NEXT_REWARD_ITEM_KEY, previousItem); // Set Next Reward Item

                // Set Stronghold update time.
                DropData dropped = new DropData();
                dropped.Block = Blockchain.GetHeight();
                dropped.HeroId = stronghold.Hero;
                dropped.ItemId = nextRewardItem.AsBigInteger();
                dropped.StrongholdId = stronghold.ID;

                byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(dropped);

                Storage.Put(Storage.CurrentContext, GeneralContract.LATEST_REWARDED_ITEM_KEY, bytes);
            }

            Runtime.Notify("Item was Dropped successfully");
            return new BigInteger(1).AsByteArray();
        }


        //------------------------------------------------------------------------------------
        //
        // City Coffers Payout
        //
        //------------------------------------------------------------------------------------
        public static byte[] CofferPayout(object[] args)
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

            // City Coffer Cut.
            for (var i = 0; i < args.Length; i++)
            {
                string key = GeneralContract.CITY_PREFIX + ((BigInteger)args[i]).AsByteArray();
                byte[] cityBytes = Storage.Get(Storage.CurrentContext, key);



                if (cityBytes.Length == 0)
                {
                    Runtime.Notify("City doesn't exist!");
                    return new BigInteger(0).AsByteArray();
                }

                City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);

                if (city.Hero == 0)
                {
                    continue;
                }

                // Coffer is 30%

                string heroKey = GeneralContract.HERO_PREFIX + city.Hero.AsByteArray();
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

                BigInteger percent = city.Coffer / 100;
                BigInteger requiredValue = percent * 30;

                foreach (var payOut in outputs)
                {
                    BigInteger incomingValue = payOut.Value;
                    if (incomingValue == requiredValue && hero.OWNER == payOut.ScriptHash)
                    {
                        city.Coffer = city.Coffer - requiredValue;

                        cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);

                        Storage.Put(Storage.CurrentContext, key, cityBytes);

                        continue;
                    }
                }

                Runtime.Notify("Hero doesn't receive his coffer prize!!!");
                return new BigInteger(0).AsByteArray();
            }

            return new BigInteger(1).AsByteArray();
        }

    }
}


