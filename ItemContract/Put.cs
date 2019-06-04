using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace LordsContract
{
    /// <summary>
    /// Class interacts with Storage
    /// </summary>
    public static class Put
    {
        ///
        /// <summary>Puts Item data on Blockchain.
        /// 
        /// IMPORTANT! Only game owner may call it.
        ///
        /// </summary>
        /// <param name="itemId">Id of Item that will be added onto storage</param>
        /// <param name="item">Item data in Structs.Item type</param>
        public static void Item(byte[] itemId, Item item, bool isInner)
        {
            // Invoker has permission to execute this function?
            if (!isInner && !Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                throw new System.Exception();
            }
            // Item should not exist.
            string key = GeneralContract.ITEM_MAP + itemId;
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);

            Runtime.Log("Item was get");

            if (bytes.Length > 0)
            {
                Runtime.Notify(8006);
                throw new System.Exception();
            }

            Runtime.Log("Item was checked");

            // Put item managable data onto blockchain 
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            
            Storage.Put(Storage.CurrentContext, key, bytes);
        }

        /// <summary>
        /// Puts hero onto blockchain
        /// </summary>
        /// <param name="heroId">Hero ID</param>
        /// <param name="hero">Her Data</param>
        /// <returns></returns>
        public static void Hero(BigInteger heroId, Hero hero)
        {

            // Put Hero
            string key = GeneralContract.HERO_MAP + heroId.ToByteArray();
            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(hero);
            Storage.Put(Storage.CurrentContext, key, bytes);
        }


        /// <summary>
        /// Method is invoked by Game Owner.
        /// </summary>
        /// <param name="id">City ID</param>
        /// <param name="size">City Size</param>
        /// <param name="cap">City's market cap</param>
        /// <returns></returns>
        public static void City(BigInteger id, BigInteger size, BigInteger cap)
        {
            // Invoker has permission to execute this function?
            if (!Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                throw new System.Exception();
            }

            if (id <= 0)
            {
                throw new System.Exception();
            }
            if (cap <= 0)
            {
                throw new System.Exception();
            }


            string key = GeneralContract.CITY_MAP + id.ToByteArray();
            byte[] cityBytes = Storage.Get(Storage.CurrentContext, key);

            if (cityBytes.Length > 0)
            {
                throw new System.Exception();
            }

            City city = new City();
            city.ID = id;
            city.Size = size;
            city.Hero = 0;          // NPC owned
            city.ItemsCap = cap;
            city.ItemsOnMarket = 0;
            city.Coffer = 0;
            city.CofferPayoutSession = 0;

            cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);

            IncrementCityAmount(id);

            Storage.Put(Storage.CurrentContext, key, cityBytes);
        }

        /// <summary>
        /// Updates amount of cities on Blockchain. This method should be called after every city putting on Blockchain
        /// </summary>
        /// <returns></returns>
        public static void IncrementCityAmount(BigInteger cityId)
        {
            BigInteger prevCityId = BigInteger.Subtract(cityId, 1);
            byte[] expectedAmountBytes = prevCityId.ToByteArray();

            byte[] amountBytes = Storage.Get(Storage.CurrentContext, GeneralContract.AMOUNT_CITIES);
            if (amountBytes.Length > 0)
            {
                if (!expectedAmountBytes.Equals(amountBytes))
                {
                    Runtime.Notify(4);
                    throw new System.Exception();
                }

                
            }
            Storage.Put(Storage.CurrentContext, GeneralContract.AMOUNT_CITIES, cityId);
        }

        /// <summary>
        /// Method is invoked by Game Owner.
        /// </summary>
        /// <param name="id">stronghold ID</param>
        /// <returns></returns>
        public static void Stronghold(BigInteger id)
        {
            // Invoker has permission to execute this function?
            if (!Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                throw new System.Exception();
            }

            if (id <= 0)
            {
                throw new System.Exception();
            }

            string key = GeneralContract.STRONGHOLD_MAP + id.ToByteArray();
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);

            if (bytes.Length > 0)
            {
                throw new System.Exception();
            }

            Stronghold stronghold = new Stronghold();
            stronghold.ID = id;
            stronghold.Hero = 0;
            stronghold.CreatedBlock = Blockchain.GetHeight();

            bytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);

            Storage.Put(Storage.CurrentContext, key, bytes);

            IncrementStrongholdAmount(id);
        }

        /// <summary>
        /// Updates amount of strongholds on Blockchain. This method should be called after every stronghold putting on Blockchain
        /// </summary>
        /// <returns></returns>
        public static void IncrementStrongholdAmount(BigInteger id)
        {
            BigInteger prevId = BigInteger.Subtract(id, 1);
            byte[] expectedAmountBytes = prevId.ToByteArray();

            byte[] amountBytes = Storage.Get(Storage.CurrentContext, GeneralContract.AMOUNT_STRONGHOLDS);
            if (amountBytes.Length > 0)
            {
                if (!expectedAmountBytes.Equals(amountBytes))
                {
                    Runtime.Notify(5);
                    throw new System.Exception();
                }
            }

            Storage.Put(Storage.CurrentContext, GeneralContract.AMOUNT_STRONGHOLDS, id);
        }

        /// <summary>
        /// Method is invoked by Game Owner.
        /// </summary>
        /// <param name="id">stronghold ID</param>
        /// <returns></returns>
        public static void BanditCamp(BigInteger id)
        {
            // Invoker has permission to execute this function?
            if (!Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                throw new System.Exception();
            }

            if (id <= 0)
            {
                throw new System.Exception();
            }

            string key = GeneralContract.BANDIT_CAMP_MAP + id.ToByteArray();
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);

            if (bytes.Length > 0)
            {
                throw new System.Exception();
            }

            Storage.Put(Storage.CurrentContext, key, 1);

            IncrementBanditCampAmount(id);
        }

        /// <summary>
        /// Updates amount of bandit camps on Blockchain. This method should be called after every bandit camp putting on Blockchain
        /// </summary>
        /// <returns></returns>
        public static void IncrementBanditCampAmount(BigInteger id)
        {
            BigInteger prevId = BigInteger.Subtract(id, 1);
            byte[] expectedAmountBytes = prevId.ToByteArray();

            byte[] amountBytes = Storage.Get(Storage.CurrentContext, GeneralContract.AMOUNT_BATTLE_CAMP);
            if (amountBytes.Length > 0)
            {
                if (!expectedAmountBytes.Equals(amountBytes))
                {
                    Runtime.Notify(5);
                    throw new System.Exception();
                }
            }

            Storage.Put(Storage.CurrentContext, GeneralContract.AMOUNT_BATTLE_CAMP, id);
        }
    }
}


