using BattleContract.Character;
using BattleContract.GameComponents;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace BattleContract.StorageData
{
    public class ItemDataHelper
    {
        public static readonly int MaxItems = 5;
        public static readonly int ItemDataLength = 30;
        public static readonly int IdLength = 13;

        public static int GetIndex(ItemDataType type)
        {
            int i = 0;  // index
            if (type==ItemDataType.Stat)
                i = 0;
            if (type==ItemDataType.MaxStat)
                i = 4;
            if (type==ItemDataType.StatType)
                i = 8;
            if (type==ItemDataType.Quality)
                i = 9;
            if (type==ItemDataType.Address)
                i = 10;
            return i;
        }
        public static int GetLength(ItemDataType type)
        {
            int i = 0;  // index
            if (type==ItemDataType.Stat)
                i = 4;
            if (type==ItemDataType.MaxStat)
                i = 4;
            if (type==ItemDataType.StatType)
                i = 1;
            if (type==ItemDataType.Quality)
                i = 1;
            if (type==ItemDataType.Address)
                i = 20;
            return i;
        }

        public static BigInteger GetValue(ItemDataType type, string parameters)
        {
            return new BigInteger(parameters.Substring(GetIndex(type), GetLength(type)).AsByteArray());
        }

        public static StatType GetStatType(string parameters)
        {
            ItemDataType type = ItemDataType.StatType;
            string param = parameters.Substring(GetIndex(type), GetLength(type));

            return Character.Helper.StringToStatType(param);
        }

        public static BigInteger GetQualityType(string parameters)
        {
            ItemDataType type = ItemDataType.Quality;
            string param = parameters.Substring(GetIndex(type), GetLength(type));

            return GameComponents.Helper.StringToQualityType(param);
        }
        public static string GetAddress(string parameters)
        {
            ItemDataType type = ItemDataType.Address;
            string param = parameters.Substring(GetIndex(type), GetLength(type));

            return param;
        }

        
        
        public static void PutItem(string itemId, BigInteger[] item, byte[] owner)
        {
            StorageContext storageContext = BattleContract.GetItemContext("getStorage", new object[] { });

            // Get the parameters
            /*string parameters = StringAndArray.Helper.GetZeroPrefixedString(item[0].AsByteArray().AsString(), 4) +
                StringAndArray.Helper.GetZeroPrefixedString(item.MaxStat.AsString(), 4) +
                GameComponents.Helper.QualityTypeToString(item.Quality) +
                Character.Helper.StatTypeToString(item.statType) +
                owner.AsString();

            Storage.Put(storageContext, item.Id.AsByteArray(), parameters);*/
        }

        /// <summary>
        /// AppCall. Get the Parameters of Data
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="address"></param>
        /// <returns>Array of 3 elements on success, that contain Value, Max Value and Quality of Item
        /// Or Array of 0 elements on failure
        /// </returns>
        public static BigInteger[] GetItem(string itemId, byte[] address)
        {
            if (itemId.Length != IdLength)
            {
                Runtime.Log("Item is not equiped");
                return new BigInteger[0];
            } else
            {
                Runtime.Log("Item is exist and required");
            }
            StorageContext storageContext = BattleContract.GetItemContext("getStorage", new object[] { });
            string parameters = Storage.Get(storageContext, itemId.AsByteArray()).AsString();
            
            // TODO. SKIP THE VALIDATION IN DEMO VERSION
            /*if (!parameters.Length.Equals(ItemDataHelper.ItemDataLength))
            {
                Runtime.Log("Item is not exist on Blockchain");
                return new BigInteger[0];
            }
            string itemOwner = ItemDataHelper.GetAddress(parameters);
            if (!itemOwner.Equals(address.AsString()))
            {
                Runtime.Log("Item is not belonging to the player);
                return new BigInteger[0];
            }*/

            BigInteger stat = GetValue(ItemDataType.Stat, parameters);
            BigInteger max = GetValue(ItemDataType.MaxStat, parameters);
            BigInteger qualityType = GetQualityType(parameters);

            BigInteger[] data = new BigInteger[3]
            {
                stat, max, qualityType
            };
            
            return data;
        }
    }
}
