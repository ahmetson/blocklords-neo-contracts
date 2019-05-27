﻿using Neo.SmartContract.Framework;
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
        public static byte[] Item(byte[] itemId, Item item, bool isInner)
        {
            // Invoker has permission to execute this function?
            if (!isInner && !Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                return new BigInteger(0).AsByteArray();
            }
            // Item should not exist.
            string key = GeneralContract.ITEM_MAP + itemId;
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);

            if (bytes.Length > 0)
            {
                Runtime.Notify(8006);
                throw new System.Exception();
            }

            // Put item managable data onto blockchain 
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            
            Storage.Put(Storage.CurrentContext, key, bytes);

            return itemId;
        }

        /// <summary>
        /// Puts hero onto blockchain
        /// </summary>
        /// <param name="heroId">Hero ID</param>
        /// <param name="hero">Her Data</param>
        /// <returns></returns>
        public static byte[] Hero(BigInteger heroId, Hero hero)
        {

            // Put Hero
            string key = GeneralContract.HERO_MAP + heroId.AsByteArray();
            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(hero);
            Storage.Put(Storage.CurrentContext, key, bytes);

            return new BigInteger(1).AsByteArray();
        }


        /// <summary>
        /// Method is invoked by Game Owner.
        /// </summary>
        /// <param name="id">City ID</param>
        /// <param name="size">City Size</param>
        /// <param name="cap">City's market cap</param>
        /// <returns></returns>
        public static byte[] City(BigInteger id, BigInteger size, BigInteger cap)
        {
            // Invoker has permission to execute this function?
            if (!Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                return new BigInteger(0).AsByteArray();
            }

            if (id <= 0)
            {
                return new BigInteger(0).AsByteArray();
            }
            if (cap <= 0)
            {
                return new BigInteger(0).AsByteArray();
            }


            string key = GeneralContract.CITY_MAP + id.AsByteArray();
            byte[] cityBytes = Storage.Get(Storage.CurrentContext, key);

            if (cityBytes.Length > 0)
            {
                return new BigInteger(0).AsByteArray();
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

            Storage.Put(Storage.CurrentContext, key, cityBytes);

            IncrementCityAmount();

            return new BigInteger(1).AsByteArray();
        }

        /// <summary>
        /// Updates amount of cities on Blockchain. This method should be called after every city putting on Blockchain
        /// </summary>
        /// <returns></returns>
        public static void IncrementCityAmount()
        {
            byte[] amountBytes = Storage.Get(Storage.CurrentContext, GeneralContract.AMOUNT_CITIES);
            BigInteger amount = 0;
            if (amountBytes.Length > 0)
            {
                amount = amountBytes.AsBigInteger();
            }
            else
            {
                amount = 0;
            }

            amount = BigInteger.Add(amount, 1);

            Storage.Put(Storage.CurrentContext, GeneralContract.AMOUNT_CITIES, amount);
        }

        /// <summary>
        /// Method is invoked by Game Owner.
        /// </summary>
        /// <param name="id">stronghold ID</param>
        /// <returns></returns>
        public static byte[] Stronghold(BigInteger id)
        {
            // Invoker has permission to execute this function?
            if (!Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                return new BigInteger(0).AsByteArray();
            }

            if (id <= 0)
            {
                return new BigInteger(0).AsByteArray();
            }

            string key = GeneralContract.STRONGHOLD_MAP + id.AsByteArray();
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);

            if (bytes.Length > 0)
            {
                return new BigInteger(0).AsByteArray();
            }

            Stronghold stronghold = new Stronghold();
            stronghold.ID = id;
            stronghold.Hero = 0;
            stronghold.CreatedBlock = Blockchain.GetHeight();

            bytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);

            Storage.Put(Storage.CurrentContext, key, bytes);

            IncrementStrongholdAmount();

            return new BigInteger(1).AsByteArray();
        }

        /// <summary>
        /// Updates amount of strongholds on Blockchain. This method should be called after every stronghold putting on Blockchain
        /// </summary>
        /// <returns></returns>
        public static void IncrementStrongholdAmount()
        {
            byte[] amountBytes = Storage.Get(Storage.CurrentContext, GeneralContract.AMOUNT_STRONGHOLDS);
            BigInteger amount = 0;
            if (amountBytes.Length > 0)
            {
                amount = amountBytes.AsBigInteger();
            }
            else
            {
                amount = 0;
            }

            amount = BigInteger.Add(amount, 1);

            Storage.Put(Storage.CurrentContext, GeneralContract.AMOUNT_STRONGHOLDS, amount);
        }

        /// <summary>
        /// Method is invoked by Game Owner.
        /// </summary>
        /// <param name="id">stronghold ID</param>
        /// <returns></returns>
        public static byte[] BanditCamp(BigInteger id)
        {
            // Invoker has permission to execute this function?
            if (!Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                return new BigInteger(0).AsByteArray();
            }

            if (id <= 0)
            {
                return new BigInteger(0).AsByteArray();
            }

            string key = GeneralContract.BANDIT_CAMP_MAP + id.AsByteArray();
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);

            if (bytes.Length > 0)
            {
                return new BigInteger(0).AsByteArray();
            }

            Storage.Put(Storage.CurrentContext, key, 1);

            IncrementBanditCampAmount();

            return new BigInteger(1).AsByteArray();
        }

        /// <summary>
        /// Updates amount of bandit camps on Blockchain. This method should be called after every bandit camp putting on Blockchain
        /// </summary>
        /// <returns></returns>
        public static void IncrementBanditCampAmount()
        {
            byte[] amountBytes = Storage.Get(Storage.CurrentContext, GeneralContract.AMOUNT_BATTLE_CAMP);
            BigInteger amount = 0;
            if (amountBytes.Length > 0)
            {
                amount = amountBytes.AsBigInteger();
            }
            else
            {
                amount = 0;
            }

            amount = BigInteger.Add(amount, 1);

            Storage.Put(Storage.CurrentContext, GeneralContract.AMOUNT_BATTLE_CAMP, amount);
        }
    }
}


