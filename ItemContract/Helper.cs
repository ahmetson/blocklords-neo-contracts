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

        
        public static void ChangeItemOwner(BigInteger itemId, BigInteger heroId)
        {
            byte[] idBytes = itemId.AsByteArray();
            string key = GeneralContract.ITEM_MAP + idBytes;
            byte[] bytes = Storage.Get(Storage.CurrentContext, key);

            Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(bytes);

            item.HERO = heroId;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);

            Put.Item(itemId, item);
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


