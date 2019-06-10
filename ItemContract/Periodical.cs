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

                    // Kick out a lord from stronghold. 
                    stronghold.Hero = 0;
                    stronghold.CreatedBlock = Blockchain.GetHeight();
                    byte[] strongholdBytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);
                    Storage.Put(Storage.CurrentContext, key, strongholdBytes);

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

        public static void PayCityCoffer(object cityId, object cityAmountObj, object paymentIntervalObj, object cofferPercentsObj)
        {
            /// Define new payment parameters if there were not any coffer payments before
            CofferPayment session = new CofferPayment();
            session.BlockStart = 1;// Blockchain.GetHeight();
            session.BlockEnd = session.BlockStart;
            session.Session = 1;
            session.AmountPaidCity = 0;

            byte[] amountCitySettingBytes = Storage.Get(Storage.CurrentContext, GeneralContract.AMOUNT_CITIES);
            byte[] cityAmountBytes = (byte[])cityAmountObj;

            if (!amountCitySettingBytes.Equals(cityAmountBytes))
            {
                Runtime.Notify(9);
                throw new System.Exception();
            }

            BigInteger amountCity = (BigInteger)cityAmountObj;

            byte[] paymentIntervalSettingBytes = Storage.Get(Storage.CurrentContext, GeneralContract.INTERVAL_COFFER);
            byte[] paymentIntervalBytes = (byte[])paymentIntervalObj;

            if (!paymentIntervalSettingBytes.Equals(paymentIntervalBytes))
            {
                Runtime.Notify(10);
                throw new System.Exception();
            }

            BigInteger paymentInterval = (BigInteger)paymentIntervalObj;

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

            BigInteger cityIdInt = (BigInteger)cityId;
            BigInteger cityCoffer = Helper.GetCoffer(cityIdInt);
            if (cityCoffer <= 0)
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
                byte[] cofferPercentsSettingBytes = Storage.Get(Storage.CurrentContext, GeneralContract.PERCENTS_COFFER_PAY);
                byte[] cofferPercentsBytes = (byte[])cofferPercentsObj;

                if (!cofferPercentsSettingBytes.Equals(cofferPercentsBytes))
                {
                    Runtime.Notify(11);
                    throw new System.Exception();
                }

                BigInteger percent = BigInteger.Divide(cityCoffer, 100);

                BigInteger payoutPercents = (BigInteger)cofferPercentsObj;

                BigInteger payoutAmount = BigInteger.Multiply(percent, payoutPercents);

                byte[] payoutAmountBytes = payoutAmount.ToByteArray();

                if (!GeneralContract.AttachmentExistAB(payoutAmountBytes, lord))
                {
                    Runtime.Notify(6004);
                    throw new System.Exception();
                }
                else
                {
                    cityCoffer = BigInteger.Subtract(cityCoffer, payoutAmount);
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
        }
    }
}


