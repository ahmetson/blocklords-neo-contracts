using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace LordsContract
{
    public static class Periodical
    {

        /**
         * Function records item drop
         * 
         * Function drops item in every 120 blocks. Usually called by Server Side of Blocklords.
         * 
         * Has 0 argument
         */
        public static void SimpleDropItem(byte[] itemId, object strongholdAmountObj, object dropIntervalObj)
        {
            DropData lastDrop = new DropData();
            lastDrop.Block = 0;
            lastDrop.StrongholdId = 0;

            byte[] lastDropBytes = Storage.Get(Storage.CurrentContext, GeneralContract.LAST_ITEM_DROP);
            if (lastDropBytes.Length > 0)
            {
                lastDrop = (DropData)Neo.SmartContract.Framework.Helper.Deserialize(lastDropBytes);
            }

            byte[] dropIntervalSettingBytes = Storage.Get(Storage.CurrentContext, GeneralContract.INTERVAL_DROP);
            byte[] dropIntervalBytes = (byte[])dropIntervalObj;

            if (dropIntervalSettingBytes.Length > 0)
            {
                if (!dropIntervalSettingBytes.Equals(dropIntervalBytes))
                {
                    Runtime.Notify(7);
                    throw new System.Exception();
                }
            }

            BigInteger dropInterval = (BigInteger)dropIntervalObj;

            if (Blockchain.GetHeight() <= dropInterval + lastDrop.Block)
            {
                Runtime.Notify(5001);
                throw new System.Exception();
            }

            byte[] strongholdsAmountSettingBytes = Storage.Get(Storage.CurrentContext, GeneralContract.AMOUNT_STRONGHOLDS);
            byte[] strongholdsAmountBytes = (byte[])strongholdAmountObj;

            if (!strongholdsAmountSettingBytes.Equals(strongholdsAmountBytes))
            {
                Runtime.Notify(8);
                throw new System.Exception();
            }

            BigInteger strongholdsAmount = (BigInteger)strongholdAmountObj;

            string key;
            Stronghold stronghold;
            byte[] bytes;


            // Check that Item has no owner and that is is on stronghold reward batch
            string itemKey = GeneralContract.ITEM_MAP + itemId;
            bytes = Storage.Get(Storage.CurrentContext, itemKey);
            if (bytes.Length <= 0)
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
            BigInteger random = GeneralContract.GetRandomNumber(0, strongholdsAmount);
            random = BigInteger.Add(random, 1);

            key = GeneralContract.STRONGHOLD_MAP + random.ToByteArray();
            bytes = Storage.Get(Storage.CurrentContext, key);
            if (bytes.Length <= 0)
            {
                // Delete Item
                //Storage.Delete(Storage.CurrentContext, itemKey);
                Runtime.Notify(5003);//, itemId, random, 0, Blockchain.GetHeight());
                throw new System.Exception();
            }
            else
            {
                stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(bytes);

                BigInteger lordId = stronghold.Hero;
                if (lordId <= 0)
                {
                    // Delete Item
                    Storage.Delete(Storage.CurrentContext, itemKey);

                    // Record the match
                    lastDrop.Block = Blockchain.GetHeight();
                    lastDrop.HeroId = 0;
                    lastDrop.ItemId = itemId;
                    lastDrop.StrongholdId = random;

                    lastDropBytes = Neo.SmartContract.Framework.Helper.Serialize(lastDrop);

                    Storage.Put(Storage.CurrentContext, GeneralContract.LAST_ITEM_DROP, lastDropBytes);

                    Runtime.Notify(5004, itemId, random, 0, Blockchain.GetHeight());
                }
                else
                {

                    string heroKey = GeneralContract.HERO_MAP + lordId.ToByteArray();
                    Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, heroKey));

                    // Change owner of Item.
                    item.HERO = hero.ID;
                    item.BATCH = GeneralContract.NO_BATCH;
                    byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

                    // Save Item after change of ownership
                    Storage.Put(Storage.CurrentContext, itemKey, itemBytes);

                    // Delete Stronghold owner. (It means "kicking out from Stronghold"). According to rule, if stronghold owner got item, he should be kicked out from stronghold
                    Storage.Delete(Storage.CurrentContext, key);

                    lastDrop.Block = Blockchain.GetHeight();
                    lastDrop.HeroId = hero.ID;
                    lastDrop.ItemId = itemId;
                    lastDrop.StrongholdId = random;

                    lastDropBytes = Neo.SmartContract.Framework.Helper.Serialize(lastDrop);

                    Storage.Put(Storage.CurrentContext, GeneralContract.LAST_ITEM_DROP, lastDropBytes);

                    // Save hero with the mark that he lost his stronghold
                    hero.StrongholdsAmount = BigInteger.Subtract(hero.StrongholdsAmount, 1);
                    byte[] heroBytes = Neo.SmartContract.Framework.Helper.Serialize(hero);
                    Storage.Put(heroKey, heroBytes);

                    Runtime.Notify(5000, itemId, lastDrop.StrongholdId, lastDrop.HeroId, lastDrop.Block);
                }
            }
        }
   
        public static byte[] PayCityCoffer(byte[] cityId, object cityAmountObj, object paymentIntervalObj)
        {
            /// Define new payment parameters if there were not any coffer payments before
            CofferPayment session = new CofferPayment();
            session.BlockStart = 1;// Blockchain.GetHeight();
            session.BlockEnd = session.BlockStart;
            session.Session = 1;
            session.AmountPaidCity = 0;

            byte[] amountCityBytes = Storage.Get(Storage.CurrentContext, GeneralContract.AMOUNT_CITIES);
            BigInteger amountCity = amountCityBytes.ToBigInteger();

            byte[] paymentIntervalBytes = Storage.Get(Storage.CurrentContext, GeneralContract.INTERVAL_COFFER);
            BigInteger paymentInterval = paymentIntervalBytes.ToBigInteger();

            byte[] lastCofferSession = Storage.Get(Storage.CurrentContext, GeneralContract.COFFER_PAYMENT_SESSION);
            if (lastCofferSession.Length > 0)
            {
                session = (CofferPayment)Neo.SmartContract.Framework.Helper.Deserialize(lastCofferSession);
                
            }

            /// Last Session is fully payed out
            if (session.AmountPaidCity >= amountCity)
            {
                /// It's not a time to open a new session
                if (Blockchain.GetHeight() < session.BlockEnd + paymentInterval)
                {
                    Runtime.Notify(6001);
                    throw new System.Exception();
                }

                BigInteger newSession = BigInteger.Add(session.Session, 1);

                /// Define a new session
                session = new CofferPayment();
                session.BlockStart = Blockchain.GetHeight();
                session.BlockEnd = session.BlockStart;
                session.Session = newSession;
                session.AmountPaidCity = 0;
            }

            string cityKey = GeneralContract.CITY_MAP + cityId;
            byte[] cityBytes = Storage.Get(Storage.CurrentContext, cityKey);
            if (cityBytes.Length <= 0)
            {
                Runtime.Notify(1003);
                throw new System.Exception();
            }

            City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);
            if (city.CofferPayoutSession >= session.Session)
            {
                Runtime.Notify(6003);
                throw new System.Exception();
            }

            byte[] lord = GeneralContract.GameOwner;
            if (city.Hero > 0)
            {
                BigInteger cityLordId = city.Hero;
                string heroKey = GeneralContract.HERO_MAP + cityLordId.AsByteArray();
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);

                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
                lord = hero.OWNER;
            }

            if (city.Coffer <= 0)
            {
                city.CofferPayoutSession = session.Session;
                cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
                Storage.Put(Storage.CurrentContext, cityKey, cityBytes);

                
                session.AmountPaidCity = BigInteger.Add(session.AmountPaidCity, 1);
                session.BlockEnd = Blockchain.GetHeight();

                lastCofferSession = Neo.SmartContract.Framework.Helper.Serialize(session);

                if (session.AmountPaidCity >= amountCity)
                {

                    BigInteger sessionId = session.Session;
                    string paymentKey = GeneralContract.COFFER_PAYMENT_SESSION_MAP + sessionId.ToByteArray();
                    Storage.Put(Storage.CurrentContext, paymentKey, lastCofferSession);
                }

                Storage.Put(Storage.CurrentContext, GeneralContract.COFFER_PAYMENT_SESSION, lastCofferSession);
            }
            else
            {
                BigInteger percent = BigInteger.Divide(city.Coffer, 100);

                byte[] cofferPercentsBytes = Storage.Get(Storage.CurrentContext, GeneralContract.PERCENTS_COFFER_PAY);
                BigInteger cofferPercents = cofferPercentsBytes.ToBigInteger();

                BigInteger cofferPaymentSize = BigInteger.Multiply(percent, cofferPercents);

                if (!GeneralContract.AttachmentExist(cofferPaymentSize, lord))
                {
                    Runtime.Notify(6004);
                    throw new System.Exception();
                }
                else
                {
                    city.Coffer = BigInteger.Subtract(city.Coffer, cofferPaymentSize);
                    city.CofferPayoutSession = session.Session;

                    cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
                    Storage.Put(Storage.CurrentContext, cityKey, cityBytes);

                    session.AmountPaidCity = BigInteger.Add(session.AmountPaidCity, 1);
                    session.BlockEnd = Blockchain.GetHeight();

                    lastCofferSession = Neo.SmartContract.Framework.Helper.Serialize(session);

                    if (session.AmountPaidCity >= amountCity)
                    {

                        BigInteger sessionId = session.Session;
                        string paymentKey = GeneralContract.COFFER_PAYMENT_SESSION_MAP + sessionId.ToByteArray();
                        Storage.Put(Storage.CurrentContext, paymentKey, lastCofferSession);
                    }

                    Storage.Put(Storage.CurrentContext, GeneralContract.COFFER_PAYMENT_SESSION, lastCofferSession);
                }

                Runtime.Notify(6000, cityId, session.Session, session.AmountPaidCity, amountCity);
            }

            byte[] res = new byte[1] { 0 };
            return res;
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
        //        dropped.ItemId = nextRewardItem.ToBigInteger();
        //        dropped.StrongholdId = stronghold.ID;

        //        byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(dropped);

        //        Storage.Put(Storage.CurrentContext, GeneralContract.LATEST_REWARDED_ITEM_KEY, bytes);
        //    }

        //    Runtime.Notify("Item was Dropped successfully");
        //    return new BigInteger(1).AsByteArray();
        //}
    }
}


