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
            DropData strongholdReward = new DropData();
            strongholdReward.Block = 0;
            strongholdReward.StrongholdId = 0;

            byte[] lastStrongholdRewardBytes = Storage.Get(Storage.CurrentContext, GeneralContract.STRONGHOLD_REWARD);
            if (lastStrongholdRewardBytes.Length > 0)
            {
                strongholdReward = (DropData)Neo.SmartContract.Framework.Helper.Deserialize(lastStrongholdRewardBytes);
            }

            byte[] rewardIntervalBytes = Storage.Get(Storage.CurrentContext, GeneralContract.INTERVAL_STRONGHOLD_REWARD);
            BigInteger rewardInterval = rewardIntervalBytes.AsBigInteger();

            if (Blockchain.GetHeight() <= rewardInterval + strongholdReward.Block)
            {
                Runtime.Notify(5001);
                throw new System.Exception();
            }

            byte[] strongholdsAmountBytes = Storage.Get(Storage.CurrentContext, GeneralContract.AMOUNT_STRONGHOLDS);
            BigInteger strongholdsAmount = strongholdsAmountBytes.AsBigInteger();

            string key;
            BigInteger checkedId = 1;
            Stronghold stronghold;
            byte[] bytes;


            // Check that Item has no owner and that is is on stronghold reward batch
            string itemKey = GeneralContract.ITEM_MAP + itemId.AsByteArray();
            bytes = Storage.Get(Storage.CurrentContext, itemKey);
            if (bytes.Length < 1)
            {
                Runtime.Notify(1005);
                throw new System.Exception();
            }
            Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(bytes);
            if (item.BATCH != GeneralContract.STRONGHOLD_REWARD_BATCH)
            {
                Runtime.Notify(5002);
                throw new System.Exception();
            }

            // returned an index on list of available strongholds ids
            BigInteger random = GeneralContract.GetRandomNumber(0, (ulong)strongholdsAmount);
            random = BigInteger.Add(random, 1);

            key = GeneralContract.STRONGHOLD_MAP + random.AsByteArray();
            bytes = Storage.Get(Storage.CurrentContext, key);
            if (bytes.Length <= 0)
            {
                // Delete Item
                Storage.Delete(Storage.CurrentContext, itemKey);
                Runtime.Notify(5003, itemId, random, 0, Blockchain.GetHeight());
                return new BigInteger(1).AsByteArray();
            }
            else
            {
                stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(bytes);

                BigInteger lordId = stronghold.Hero;
                if (lordId < 1)
                {
                    // Delete Item
                    Storage.Delete(Storage.CurrentContext, itemKey);

                    Runtime.Notify(5004, itemId, random, 0, Blockchain.GetHeight());
                    return new BigInteger(1).AsByteArray();
                }
                else
                {

                    string heroKey = GeneralContract.HERO_MAP + lordId.AsByteArray();
                    Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, heroKey));

                    // Change owner of Item.
                    item.HERO = hero.ID;
                    item.BATCH = GeneralContract.NO_BATCH;
                    byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

                    // Save Item
                    Storage.Put(Storage.CurrentContext, itemKey, itemBytes);

                    // Delete Stronghold owner. (It means "kicking out from Stronghold"). According to rule, if stronghold owner got item, he should be kicked out from stronghold
                    Storage.Delete(Storage.CurrentContext, key);

                    strongholdReward.Block = Blockchain.GetHeight();
                    strongholdReward.HeroId = hero.ID;
                    strongholdReward.ItemId = itemId;
                    strongholdReward.StrongholdId = random;

                    lastStrongholdRewardBytes = Neo.SmartContract.Framework.Helper.Serialize(strongholdReward);

                    Storage.Put(Storage.CurrentContext, GeneralContract.STRONGHOLD_REWARD, lastStrongholdRewardBytes);

                    Runtime.Notify(5000, itemId, strongholdReward.StrongholdId, strongholdReward.HeroId, strongholdReward.Block);
                }
            }
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


   
        public static byte[] PayCityCoffer(BigInteger cityId)
        {
            CofferPayment payment = new CofferPayment();
            payment.BlockStart = Blockchain.GetHeight();
            payment.BlockEnd = payment.BlockStart;
            payment.Session = 1;
            payment.AmountPaidCity = 0;

            byte[] amountCityBytes = Storage.Get(Storage.CurrentContext, GeneralContract.AMOUNT_CITIES);
            BigInteger amountCity = amountCityBytes.AsBigInteger();

            byte[] paymentIntervalBytes = Storage.Get(Storage.CurrentContext, GeneralContract.INTERVAL_COFFER);
            BigInteger paymentInterval = paymentIntervalBytes.AsBigInteger();

            byte[] lastCofferSession = Storage.Get(Storage.CurrentContext, GeneralContract.COFFER_PAYMENT_SESSION);
            if (lastCofferSession.Length > 0)
            {
                payment = (CofferPayment)Neo.SmartContract.Framework.Helper.Deserialize(lastCofferSession);
                
            }

            if (payment.AmountPaidCity >= amountCity)
            {
                if (Blockchain.GetHeight() < payment.BlockEnd + paymentInterval)
                {
                    Runtime.Notify(6001);
                    throw new System.Exception();
                }

                BigInteger newSession = BigInteger.Add(payment.Session, 1);

                payment = new CofferPayment();
                payment.BlockStart = Blockchain.GetHeight();
                payment.BlockEnd = payment.BlockStart;
                payment.Session = newSession;
                payment.AmountPaidCity = 0;
            }

            string cityKey = GeneralContract.CITY_MAP + cityId.AsByteArray();
            byte[] cityBytes = Storage.Get(Storage.CurrentContext, cityKey);
            if (cityBytes.Length <= 0)
            {
                Runtime.Notify(1003);
                throw new System.Exception();
            }

            City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);
            if (city.CofferPayoutSession >= payment.Session)
            {
                Runtime.Notify(6003);
                throw new System.Exception();
            }

            byte[] lord = GeneralContract.GameOwner;
            if (city.Hero > 0)
            {
                string heroKey = GeneralContract.HERO_MAP + city.Hero.AsByteArray();
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);

                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
                lord = hero.OWNER;
            }

            if (city.Coffer <= 0)
            {
                city.CofferPayoutSession = payment.Session;
                cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
                Storage.Put(Storage.CurrentContext, cityKey, cityBytes);

                payment.AmountPaidCity = BigInteger.Add(payment.AmountPaidCity, 1);

                if (payment.AmountPaidCity >= amountCity)
                {
                    payment.BlockEnd = Blockchain.GetHeight();
                    string paymentKey = GeneralContract.COFFER_PAYMENT_SESSION_MAP + payment.Session.AsByteArray();
                    byte[] paymentBytes = Neo.SmartContract.Framework.Helper.Serialize(payment);
                    Storage.Put(Storage.CurrentContext, paymentKey, paymentBytes);
                }
            }
            else
            {
                BigInteger percent = BigInteger.Divide(city.Coffer, 100);

                byte[] cofferPercentsBytes = Storage.Get(Storage.CurrentContext, GeneralContract.PERCENTS_COFFER_PAY);
                BigInteger cofferPercents = cofferPercentsBytes.AsBigInteger();

                BigInteger cofferPaymentSize = BigInteger.Multiply(percent, cofferPercents);

                if (!GeneralContract.AttachmentExist(cofferPaymentSize, lord))
                {
                    Runtime.Notify(6004);
                    throw new System.Exception();
                }
                else
                {
                    city.Coffer = BigInteger.Subtract(city.Coffer, cofferPaymentSize);
                    city.CofferPayoutSession = payment.Session;

                    cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
                    Storage.Put(Storage.CurrentContext, cityKey, cityBytes);

                    payment.AmountPaidCity = BigInteger.Add(payment.AmountPaidCity, 1);
                    payment.BlockEnd = Blockchain.GetHeight();

                    lastCofferSession = Neo.SmartContract.Framework.Helper.Serialize(payment);

                    if (payment.AmountPaidCity >= amountCity)
                    {
                        
                        string paymentKey = GeneralContract.COFFER_PAYMENT_SESSION_MAP + payment.Session.AsByteArray();
                        Storage.Put(Storage.CurrentContext, paymentKey, lastCofferSession);
                    }

                    Storage.Put(Storage.CurrentContext, GeneralContract.COFFER_PAYMENT_SESSION, lastCofferSession);
                }

                Runtime.Notify(6000, cityId, payment.Session, payment.AmountPaidCity, amountCity);
            }
            return new BigInteger(0).AsByteArray();
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


