using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace ItemContract
{
    public class Contract1 : SmartContract
    {
        public static void Main()
        {
            Storage.Put(Storage.CurrentContext, "Hello", "World");
        }

        [Serializable]
        public class MarketItemData
        {
            public BigInteger StartPrice;
            public BigInteger EndPrice;
            public BigInteger AuctionDuration;
            public BigInteger AuctionStartedBlock;
            public byte[] Seller;
        }

        [Serializable]
        public class ItemStaticData
        {

        }

        [Serializable]
        public class ItemData
        {
            public ItemStaticData Static;
            public ItemEditableData Editable;
        }

        [Serializable]
        public class ItemEditableData
        {

        }

        [Serializable]
        public class DropData
        {
            public BigInteger InsertedInitialItems;
            public BigInteger GivenItems;
            public BigInteger InsertedDroppableItems;
            public BigInteger DroppedItems;
            public BigInteger DropInterval;
            public BigInteger LastDroppedBlock;
            public BigInteger DroppedStrongholdId;
        }

        /**
         * Storage
         */
        //static readonly string ItemDataList = "ItemDataList";     // Actually items in the list is built as a [ DROP_ID => Item Data ]
        //static readonly string Market = "Market";                 // Actually market items list is as a [ ID => Market Data ]
        static readonly string DropParameters = "Drop";
        //static readonly string ItemEditableFields;                         // [ Owner ID => Item Data ]
        //static readonly string HeroDroppedItem = "LastDroppedItem";
        //static raedonly string StrongholdDroopedItem = "Stronghold";      // key => "Item ID_Stronghold ID"

        //------------------------------------------------------------------------------------
        //
        // AUCTION
        //
        //------------------------------------------------------------------------------------

        public static void AuctionBegin()
        {

        }

        public static void AuctionEnd()
        {

        }

        public static void AuctionCancel()
        {

        }

        //------------------------------------------------------------------------------------
        //
        // GAME OWNER
        //
        //------------------------------------------------------------------------------------

        public static void AddItem()
        {

        }

        public static void UpdateDropData()
        {

        }

        //------------------------------------------------------------------------------------
        //
        // ITEM DROPS
        //
        //------------------------------------------------------------------------------------
           
        public static void GiveItems()
        {

        }

        public static void DropItems()
        {

        }

        //------------------------------------------------------------------------------------
        //
        // Item Edit
        //
        //------------------------------------------------------------------------------------

        public static void UpdateItemStats(BigInteger[] ids, BigInteger[] newStats)
        {

        }
    }
}
