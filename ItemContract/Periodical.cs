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
            
        }
    }
}


