using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System.Numerics;

namespace LordsContract
{
    public static class Helper
    {
        
        public static bool IsItemAvailable(BigInteger itemId, byte batchType)
        {
            if (itemId == 0)
            {
                return true;
            }

            byte[] idBytes = itemId.ToByteArray();
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

        
        public static void ChangeItemOwner(byte[] itemId, BigInteger heroId)
        {
            string key = GeneralContract.ITEM_MAP + itemId;
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);

            if (bytes.Length <= 0)
            {
                Runtime.Notify(1005);
                throw new System.Exception();
            }

            Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(bytes);
            if (item.BATCH != GeneralContract.HERO_CREATION_BATCH)
            {
                Runtime.Notify(4012);
                throw new System.Exception();
            }

            item.HERO = heroId;
            item.BATCH = GeneralContract.NO_BATCH;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);

            //Put.Item(itemId, item, true);
            Storage.Put(Storage.CurrentContext, key, bytes);
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

        public static byte[] GetIdByIndex(byte[][] arr, BigInteger arrLength, BigInteger index)
        {
            BigInteger passedIndex = 0;
            for (int i = 0; i < arrLength; i++, passedIndex = passedIndex + 1)
            {
                if (index == passedIndex)
                {
                    return arr[i];
                }
            }

            return new byte[0];
        }

    }
}


