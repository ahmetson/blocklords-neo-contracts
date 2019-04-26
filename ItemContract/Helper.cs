using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System.Numerics;

namespace LordsContract
{
    public static class Helper
    {
        //------------------------------------------------------------------------------------
        //
        // functions for:
        // GAME OBJECT CREATION
        //
        //------------------------------------------------------------------------------------
        public static byte[] SerializeVerificationalData(Item item)
        {
            byte[] emptyField = new byte[] { 0 };

            byte[] delimeter = new byte[] { 0, 0 };     // Two zeros mean delimeter of parameters

            byte[] res;//item.LENGTH;

            byte[] generation = new byte[] { 0 }; //BigInteger i = new BigInteger(generation.Length); lengths[0] = Int2byte(i);
            byte[] level = new byte[] { 0 };// lengths[2] = Int2byte(new BigInteger(level.Length));
            byte[] quality = new byte[] { 0 }; //lengths[3] = Int2byte(new BigInteger(quality.Length));
            byte[] statType = new byte[] { 0 }; //lengths[4] = Int2byte(new BigInteger(statType.Length));
            byte[] statValue = new byte[] { 0 }; //lengths[5] = Int2byte(new BigInteger(statValue.Length));
            byte[] xp = new byte[] { 0 };

            if (item.GENERATION != 0)
            {
                generation = item.GENERATION.AsByteArray();
            }
            if (item.LEVEL != 0)
            {
               level = item.LEVEL.AsByteArray();
            }
            if (item.QUALITY != 0)
            {
                quality = item.QUALITY.AsByteArray();
            }
            if (item.STAT_TYPE != 0)
            {
                statType = item.STAT_TYPE.AsByteArray();
            }
            if (item.STAT_VALUE != 0)
            {
                statValue = item.STAT_VALUE.AsByteArray();
            }
            if (item.XP != 0)
            {
                xp = item.XP.AsByteArray();
            }

            res = generation;
            res = res.Concat(delimeter);
            res = res.Concat(level);
            res = res.Concat(delimeter);
            res = res.Concat(level);
            res = res.Concat(delimeter);
            res = res.Concat(level);
            res = res.Concat(delimeter);
            res = res.Concat(level);
            res = res.Concat(delimeter);
            res = res.Concat(level);
            res = res.Concat(delimeter);
            res = res.Concat(item.OWNER);

            return res;
        }

        public static bool IsItemAvailable(BigInteger itemId, byte batchType)
        {
            if (itemId == 0)
            {
                return true;
            }

            byte[] idBytes = itemId.AsByteArray();
            string key = GeneralContract.ITEM_MAP + idBytes;

            // If Item's are not exist, exit
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);
            if (bytes.Length == 0)
            {
                return false;
            }

            Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(bytes);

            return item.BATCH == batchType;
        }

        
        public static void ChangeItemOwner(BigInteger itemId, byte[] owner)
        {
            byte[] idBytes = itemId.AsByteArray();
            string key = GeneralContract.ITEM_MAP + idBytes;
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);

            Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(bytes);

            item.OWNER = owner;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);

            Put.Item(itemId, item);
        }

        public static BigInteger GetDropIncrementor()
        {
            byte[] bytes = Storage.Get(Storage.CurrentContext, GeneralContract.DROPPED_INCREMENTOR);

            if (bytes.Length == 0)
            {
                return 1;
            }

            BigInteger incrementor = bytes.AsBigInteger();

            return incrementor;
        }

        public static void SetDropIncrementor(BigInteger incrementor)
        {
            Runtime.Log("Setting Drop Inrecemntor");
            byte[] number = incrementor.AsByteArray();
            Runtime.Log("Converted to S");
            Storage.Put(Storage.CurrentContext, GeneralContract.DROPPED_INCREMENTOR, number);
            Runtime.Log("Incrementor");
        }

        public static BigInteger GetByIntIndex(BigInteger[] arr, int arrLength, BigInteger index)
        {
            BigInteger passedIndex = 0;
            for (int i = 0; i<arrLength; i++, passedIndex = passedIndex + 1)
            {
                if (index == passedIndex)
                {
                    return arr[i];
                }
            }

            return 0;
        }
    }
}


