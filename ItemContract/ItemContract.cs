using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System.Numerics;

/**
 * The ItemContract works with the Items/Equipments in the Blocklords
*  Items increase the basic Stats of heroes.
*  
*  Version: 1.0
*  Author: Medet Ahmetson
*  Date: 20 Jul. 2018 
*   
*  Item parameters are:
*  ITEM ID (13)    STAT VALUE (4)  MAX STAT VALUE (4)   STAT TYPE (1) QUALITY (1)  OWNER ADDRESS (20)
*/
namespace Blocklords
{
    public class ItemContract : SmartContract
    {
        // Item Parameters Locations
        private static readonly int statValueIndex      = 0;  //0123
        private static readonly int maxStatValueIndex   = 4;  //4567
        private static readonly int statTypeIndex       = 8;  //8
        private static readonly int qualityIndex        = 9;  //9
        private static readonly int addressIndex        = 10; //10
        private static readonly int statValueLength     = 4;
        private static readonly int maxStatValueLength  = 4;
        private static readonly int statTypeLength      = 1;
        private static readonly int qualityLength       = 1;
        private static readonly int addressLength       = 20;
        private static readonly int itemIdLength        = 13;

        // 0.01 GAS
        private static readonly decimal fee = 0.01m;

        private static byte[] GetFalseByte(string message)
        {
            Runtime.Log(message);
            return new BigInteger(0).AsByteArray();
        }
        private static byte[] GetTrueByte(string message)
        {
            string log = "Success:" + message;
            Runtime.Log(log);
            return new BigInteger(1).AsByteArray();
        }

        public static object Main(string operation, object[] args)
        {
            /*if (operation == "put" || 
                  operation == "update" ||
                  operation == "transfer" )
            {
                if (!Runtime.CheckWitness((byte[])args[0]))
                {
                    Runtime.Log("Authorization failed!");
                    return GetFalseByte();
                }
            }*/
            Runtime.Log("Version:0.1.7");

            // @Param Item ID
            if (operation.Equals("get"))                 return Get((string)args[0]);

            // @Param Item ID
            if (operation.Equals("getStorage")) return Storage.CurrentContext;

            // @Param Item ID, Item Parameters
            if (operation.Equals("put"))                 return Put((byte[])args[0], (string)args[1], (string)args[2]);

            // @Param Item ID, Item Parameters
            //if (operation.Equals("increaseStat"))              return IncreaseStat((string)args[1], new BigInteger((int)args[2]));

            // @Param Item ID, From Address, To Address
            if (operation.Equals("transfer"))            return Transfer((byte[])args[0], (string)args[1]);

            return GetFalseByte("invalid_operation");
        }

        // OPERATIONS
        private static byte[] Get(string itemId)
        {
            // Validate input
            if (!IsValidItemId(itemId))
            {
                return GetFalseByte("invalid_item_id");
            }

            byte[] item = Storage.Get(Storage.CurrentContext, itemId);
            Runtime.Log("Item is "+item.AsString());

            return item;
        }
        private static byte[] Put(byte[] scriptHash, string itemId, string itemParams)
        {
            Runtime.Log("Creating a new Item");
            // Validate input
            if (!IsValidItemId(itemId))
            {
                return GetFalseByte("invalid_item_id");
            }
            if (!IsValidItemParams(itemParams))
            {
                return GetFalseByte("invalid_item_parameters");
            }

            /*if (!IsTransactionFeeIncluded())
            {
                Runtime.Log("Required to include the Item fee!");
                return GetFalseByte();
            }*/
            Runtime.Log("Item Parameters are validated!");
            string ownerAddress = scriptHash.AsString();
            string itemParameters = itemParams + ownerAddress;
            Storage.Put(Storage.CurrentContext, itemId, itemParameters);

            return GetTrueByte("Item was put on the storage");
        }
        /*private static byte[] IncreaseStat(string itemId, BigInteger increaseValue)
        {
            string item = Storage.Get(Storage.CurrentContext, itemId).AsString();
            // Check does Item exists
            if (!IsValidItemParams(item))
            {
                return GetFalseByte("item_not_exist");
            }

            string valueString = item.Substring(ItemContract.statValueIndex, ItemContract.statValueLength);
            string maxValueString = item.Substring(ItemContract.maxStatValueIndex, ItemContract.maxStatValueLength);
            BigInteger value = valueString.AsByteArray().AsBigInteger();
            BigInteger maxValue = maxValueString.AsByteArray().AsBigInteger();

            BigInteger newValue = value + increaseValue;

            if (newValue > maxValue)
            {
                return GetFalseByte("over_max_stat_value");
            }

            string newValueString = GetZeroPrefixedString(newValue.AsByteArray().AsString(), ItemContract.statValueLength);
            string newItem = newValueString + item.Substring(ItemContract.maxStatValueIndex, ItemContract.addressIndex + 1 + ItemContract.addressLength);

            return PutOnStorage(itemId, newItem);
        }
        */
        private static byte[] Transfer(byte[] toAddress, string itemId)
        {
            Runtime.Log("Transfering item");
            // Validate input
            if (!IsValidItemId(itemId))
            {
                return GetFalseByte("invalid_item_id");
            }
            /*if (!IsValidWalletAddress(toAddress))
            {
                return GetFalseByte();
            }*/
            if (!IsTransactionFeeIncluded())
            {
                return GetFalseByte("transaction_fee_not_included");
            }
            Runtime.Log("Parameters are validated");
           
            string fromItem = Storage.Get(Storage.CurrentContext, itemId).AsString();
            // Check does Item exist
            if (!IsValidItemParams(fromItem))
            {
                return GetFalseByte("item_not_exist");
            }

            string fromAddress = fromItem.Substring(addressIndex, addressLength);
            if (fromAddress.Equals(toAddress))
            {
                return GetFalseByte("transfer_to_yourself");
            }

            Runtime.Log("Item owner will be changed");

            string toItem = fromItem.Substring(0, ItemContract.addressIndex) + toAddress.AsString();

            return ItemContract.PutOnStorage(itemId, toItem);
        }

        // VALIDATORS
        private static bool IsTransactionFeeIncluded()
        {
            TransactionOutput[] outputs = ((Transaction)ExecutionEngine.ScriptContainer).GetOutputs();
            foreach (TransactionOutput output in outputs)
            {
                if (output.ScriptHash.Equals(ExecutionEngine.EntryScriptHash))
                {
                    /*if (reference.AssetId.Equals(inputId))
                    {
                        return 0;
                    }
                    else
                    {*/
                    long value = output.Value;
                    if (value.Equals(ItemContract.fee)) {
                        return true;
                    }
                   // }
                }
            }

            return false;
        }
        private static bool IsValidItemId(string itemId)
        {
            int length = itemId.Length;
            return ItemContract.itemIdLength.Equals(length);
        }
        private static bool IsValidItemParams(string itemParams)
        {
            int itemLength = ItemContract.addressIndex;
            int itemParamsLength = itemParams.Length;
            return itemLength.Equals(itemParamsLength);
        }
        private static bool IsValidWalletAddress(string address)
        {
            return address.Length.Equals(ItemContract.addressLength);
        }
    
        // HELPERS
        private static string GetZeroPrefixedString(string toPrefix, int length)
        {
            if (toPrefix.Length.Equals(length))
            {
                return toPrefix;
            }

            string str = "";
            int prefixesNumber = length - toPrefix.Length;

            // Set Zero Prefixes
            for (int i=0; i<prefixesNumber; i++)
            {
                str = str + "0";
            }

            // Set String Value after Prefixes
            str = str + toPrefix;

            return str;
        }
        private static byte[] PutOnStorage(string itemId, string itemParams)
        {
            Storage.Put(Storage.CurrentContext, itemId, itemParams);

            return GetTrueByte("item_was_put_on_storage");
        }
    }
}
