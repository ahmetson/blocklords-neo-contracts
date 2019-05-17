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
        public static byte[] Item(BigInteger itemId, Item item, bool isInner)
        {
            if (itemId <= 0)
            {
                Runtime.Log("Item ID must be greater than 0!");
                return new BigInteger(0).AsByteArray();
            }
            // Invoker has permission to execute this function?
            if (!isInner && !Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                Runtime.Log("Permission denied! Only game admin can use this function!");
                return new BigInteger(0).AsByteArray();
            }
            // Item should not exist.
            byte[] idBytes = itemId.AsByteArray();
            string key = GeneralContract.ITEM_MAP + idBytes;
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);

            if (bytes.Length > 0)
            {
                Runtime.Log("Item should not exist on Blockchain!");
                return new BigInteger(0).AsByteArray();
            }

            // Put item managable data onto blockchain 
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            
            Storage.Put(Storage.CurrentContext, key, bytes);

            Runtime.Notify("Item was successfully put to storage");
            return new BigInteger(1).AsByteArray();
        }

        /// <summary>
        /// Puts hero onto blockchain
        /// </summary>
        /// <param name="heroId">Hero ID</param>
        /// <param name="hero">Her Data</param>
        /// <returns></returns>
        public static byte[] Hero(BigInteger heroId, Hero hero)
        {
            // Hero should not exist
            // Hero items should not belong to someone
            // Hero items should not be on stronghold reward batch
            // Player should not have have a created hero
            // Attachment should be valid

            //if (!Helper.IsItemAvailable(hero.Equipments[0], GeneralContract.HERO_CREATION_BATCH))
            //{
            //    Runtime.Notify("Item is not exist", hero.Equipments[0]);
            //    return new BigInteger(0).AsByteArray();
            //}
            //if (!Helper.IsItemAvailable(hero.Equipments[1], GeneralContract.HERO_CREATION_BATCH))
            //{
            //    Runtime.Notify("Item is not exist", hero.Equipments[1]);
            //    return new BigInteger(0).AsByteArray();
            //}
            //if (!Helper.IsItemAvailable(hero.Equipments[2], GeneralContract.HERO_CREATION_BATCH))
            //{
            //    Runtime.Notify("Item is not exist", hero.Equipments[2]);
            //    return new BigInteger(0).AsByteArray();
            //}
            //if (!Helper.IsItemAvailable(hero.Equipments[3], GeneralContract.HERO_CREATION_BATCH))
            //{
            //    Runtime.Notify("Item is not exist", hero.Equipments[3]);
            //    return new BigInteger(0).AsByteArray();
            //}
            //if (!Helper.IsItemAvailable(hero.Equipments[4], GeneralContract.HERO_CREATION_BATCH))
            //{
            //    Runtime.Notify("Item is not exist", hero.Equipments[4]);
            //    return new BigInteger(0).AsByteArray();
            //}

            // Putting Hero costs 1 GAS for player.
            // Check attachments to Transaction, where should be sended 1 GAS to Game Owner
            //if (!GeneralContract.IsTransactionOutputExist(GeneralContract.heroCreationFee))
            //{
            //    Runtime.Log("Hero Creation Fee is not included! Hero wasn't put on Blockchain");
            //    return new BigInteger(0).AsByteArray();
            //}

            if (heroId <= 0)
            {
                Runtime.Log("Please insert id higher than 0!");
                return new BigInteger(0).AsByteArray();
            }

            // Put Hero
            string key = GeneralContract.HERO_MAP + heroId.AsByteArray();
            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(hero);
            Storage.Put(Storage.CurrentContext, key, bytes);

            Runtime.Log("Hero was created and Hero got his Items");

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
                Runtime.Log("Permission denied! Only game admin can use this function!");
                return new BigInteger(0).AsByteArray();
            }

            if (id <= 0)
            {
                Runtime.Log("City ID should be greater than 0!");
                return new BigInteger(0).AsByteArray();
            }
            if (cap <= 0)
            {
                Runtime.Log("City market cap must be greater than 0!");
                return new BigInteger(0).AsByteArray();
            }


            string key = GeneralContract.CITY_MAP + id.AsByteArray();
            byte[] cityBytes = Storage.Get(Storage.CurrentContext, key);

            if (cityBytes.Length > 0)
            {
                Runtime.Log("City is already on Blockchain");
                return new BigInteger(0).AsByteArray();
            }

            City city = new City();
            city.ID = id;
            city.Size = size;
            city.Hero = 0;          // NPC owned
            city.ItemsCap = cap;
            city.ItemsOnMarket = 0;
            city.Coffer = 0;

            cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);

            Storage.Put(Storage.CurrentContext, key, cityBytes);

            IncrementCityAmount();

            Runtime.Notify("City Information was added successfully");
            return new BigInteger(1).AsByteArray();
        }

        /// <summary>
        /// Updates amount of cities on Blockchain. This method should be called after each city putting on Blockchain
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
                Runtime.Log("Permission denied! Only game admin can use this function!");
                return new BigInteger(0).AsByteArray();
            }

            if (id <= 0)
            {
                Runtime.Log("Stronghold ID should be greater than 0!");
                return new BigInteger(0).AsByteArray();
            }

            string key = GeneralContract.STRONGHOLD_MAP + id.AsByteArray();
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);

            if (bytes.Length > 0)
            {
                Runtime.Log("Stronghold is already on Blockchain");
                return new BigInteger(0).AsByteArray();
            }

            Stronghold stronghold = new Stronghold();
            stronghold.ID = id;
            stronghold.Hero = 0;
            stronghold.CreatedBlock = Blockchain.GetHeight();

            bytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);

            Storage.Put(Storage.CurrentContext, key, bytes);

            IncrementStrongholdAmount();

            Runtime.Notify("Stronghold Information was added successfully");
            return new BigInteger(1).AsByteArray();
        }

        /// <summary>
        /// Updates amount of strongholds on Blockchain. This method should be called after each stronghold putting on Blockchain
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
    }
}


