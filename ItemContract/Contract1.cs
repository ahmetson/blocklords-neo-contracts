using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System;
using System.Numerics;

namespace LordsContract
{
    public class Contract1 : SmartContract
    {
        private static readonly string MARKET_PREFIX = "\x01\x00";
        private static readonly string ITEM_PREFIX = "\x02\x00";
        private static readonly string HERO_CREATION_PREFIX = "\x03\x00";
        private static readonly string ITEM_DROP_PREFIX = "\x04\x00";
        private static readonly string HERO_CREATION_KEY = "\x05";
        private static readonly string ITEM_DROP_KEY = "\x06";
        private static readonly string CITY_PREFIX = "\x07\x00";
        private static readonly byte HERO_CREATION_GIVEN = 0;
        private static readonly byte ITEM_DROP_GIVEN = 1;

        private static readonly BigInteger auctionFee = 5;  // In percents amount of GAS that buyers sends to Game Developers for Auction
        private static readonly BigInteger lordFee = 5;     // In percents default amount of GAS that buyers sends to City Lords for Auction 

        /**
         * 1 GAS === 100_000_000
         * 0.1GAS == 10_000_000
         */
        private static readonly decimal auction8HoursFee = 10_000_000;/*,
                                           auction12HoursFee = 20_000_000,
                                           auction24HoursFee = 30_000_000,
                                           heroCreationFee   = 30_000_000,
                                           battleFee         = 30_000_000;*/

        private static readonly BigInteger duration8Hours = 28800;
        private static readonly BigInteger duration12Hours = 43200;
        private static readonly BigInteger duration24Hours = 86400;

        private static readonly byte[] GameOwner = "AML8hyTV4vXuomovxdcAH9pRC9ny618YmA".ToScriptHash();
        private static readonly byte[] lord = "AXefwrXykUBAHHQtDxusasMMsJosvi9hhc".ToScriptHash();

        [Serializable]
        public class MarketItemData
        {
            public BigInteger Price;
            public BigInteger AuctionDuration;
            public BigInteger AuctionStartedTime;
            public byte City;                       // City ID
            public byte[] TX;
           public byte[] Seller = new byte[33];
        }


        [Serializable]
        public class Item
        {
            // STATIC DATA
            public byte STAT_TYPE;
            public byte QUALITY;
            public BigInteger GENERATION;

            // EDITABLE DATA
            public BigInteger STAT_VALUE;
            public BigInteger LEVEL;
            public byte[] OWNER;
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

        [Serializable]
        public class City
        {
            public byte[] Owner;
            public BigInteger TaxPercents;
        }

        public static byte[] Main(string param, object[] args)
        {
           
            MarketItemData marketItem = new MarketItemData();
            ///**
            // * Accepts 4 Arguments
            // * @Item ID (BigInteger)
            // * @Auction Duration (BigInteger)
            // * @Start Price (BigInteger) Multiply of 1_000_000
            // * @End Price (BigInteger) Multiply of 1_000_000
            // */
            Runtime.Log("Contract Entering >>");
            if (param.Equals("auctionBegin"))
            {
                Runtime.Log("Calling Auction Begin");
                //Runtime.Notify((MarketItemData)args[1]);

                // Testing the generation of Prefixed Keys for Storage
                //string marketKey = MARKET_PREFIX + (string)args[0]; // Since String is stored as a bytearray, we can convert it to a string too

                //Runtime.Notify("Should be <Market 0>", marketKey);

                /*BigInteger duration = new BigInteger( args[1]);
                BigInteger start = new BigInteger( args[2]);
                BigInteger end = new BigInteger( args[3]);
                */
                marketItem.AuctionDuration = (BigInteger)args[1];
                marketItem.Price = (BigInteger)args[2];
                marketItem.City = (byte)args[3];
                marketItem.AuctionStartedTime = Blockchain.GetBlock(Blockchain.GetHeight()).Timestamp;
                marketItem.Seller = (byte[])args[4];// Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash;

                string cityKey = CITY_PREFIX + marketItem.City;
                Storage.Put(Storage.CurrentContext, cityKey, lord);

                Runtime.Notify("Price is ", marketItem.Price, "Incoming price", (BigInteger)args[2]);
                Runtime.Notify("Seller is ", marketItem.Seller);

                Transaction TX = (Transaction)Neo.SmartContract.Framework.Services.System.ExecutionEngine.ScriptContainer;
                marketItem.TX = TX.Hash;
                //marketItem.AuctionStartedBlock = (BigInteger)Blockchain.GetHeight();// Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;

                //return new BigInteger(0).AsByteArray();
                return AuctionBegin((BigInteger)args[0], marketItem);
            } else if (param.Equals("putItem"))
            {
                Runtime.Log("Put Item on Storage");

                // Check Witnesses
                if (!Runtime.CheckWitness(GameOwner))
                {
                    Runtime.Log("Permission denied! Only game admin can add new items. Atleast for now!");
                    return new BigInteger(0).AsByteArray();
                }
                
                // Item given type: for hero creation or drop
                if (args.Length != 7)
                {
                    Runtime.Log("Invalid parameters");
                    return new BigInteger(0).AsByteArray();
                }

                if ((BigInteger)args[0] != ITEM_DROP_GIVEN && (BigInteger)args[0] != HERO_CREATION_GIVEN)
                {
                    Runtime.Log("Given method of Item is invalid");
                    return new BigInteger(0).AsByteArray();
                }

                // Item Parameters
                Item item = new Item();
                item.STAT_TYPE = (byte)args[2];
                item.QUALITY = (byte)args[3];
                item.GENERATION = (BigInteger)args[4];

                item.STAT_VALUE = (BigInteger)args[5];
                item.LEVEL = (BigInteger)args[6];
                item.OWNER = Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash;

                // Method puts item on storage.
                // Updates Item Given Parameters
                PutItem((BigInteger)args[1], (byte)args[0], item);
            } else if (param.Equals("heroCreated"))
            {

            } else if (param.Equals("auctionEnd"))
            {
                // Check Does item exist
                Runtime.Log("Calling Auction End");

                //return new BigInteger(0).AsByteArray();
                return AuctionEnd((BigInteger)args[0]);
            }

            //Runtime.Notify("Incorrect Parameter");
            return new BigInteger(1).AsByteArray();
        }

        /**
         * Storage
         */
        //static readonly string ItemDataList = "ItemDataList";     // Actually items in the list is built as a [ DROP_ID => Item Data ]
        //static readonly string Market = "Market";                 // Actually market items list is as a [ ID => Market Data ]
        //static readonly string DropParameters = "Drop";
        //private static MarketItemData marketItem;

        //static readonly string ItemEditableFields;                         // [ Owner ID => Item Data ]
        //static readonly string HeroDroppedItem = "LastDroppedItem";
        //static raedonly string StrongholdDroopedItem = "Stronghold";      // key => "Item ID_Stronghold ID"

        //------------------------------------------------------------------------------------
        //
        // AUCTION
        //
        //------------------------------------------------------------------------------------

        /*
         * Indicates that Item has been added to Market
         * To add Item to market, we get some transaction fee from Players
         */
        public static byte[] AuctionBegin ( BigInteger itemId, MarketItemData item )
        {
            // Check whether transaction fee is included?
            if ( ! IsAuctionTransactionFeeIncluded ( item.AuctionDuration ) )
            {
                Runtime.Notify("Error! Transaction fee is not included!");
                return new BigInteger(0).AsByteArray();
            }
            //if ( ! IsValidItem (itemId, item.Seller) )
            //{
            //    Runtime.Notify("Error! Item doesn't exist or doesn't belong to Contract caller!");
            //    return new BigInteger(0).AsByteArray();
            //}
            // Put Item on market
            string key = MARKET_PREFIX + itemId.AsByteArray();

            //item.Seller = Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash;

            byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

            Storage.Put(Storage.CurrentContext, key, itemBytes);

            //MarketItemData mItem = (MarketItemData)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));
            //Runtime.Notify("Included Item is",mItem);

            return new BigInteger(1).AsByteArray();
        }

        public static byte[] AuctionEnd(BigInteger itemId)
        {
            string key = MARKET_PREFIX + itemId.AsByteArray();
            MarketItemData mItem = (MarketItemData)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            // Calculate the Valid Data
            BigInteger requiredData = Blockchain.GetBlock(Blockchain.GetHeight()).Timestamp - (mItem.AuctionDuration * 3600);
            if (mItem.AuctionStartedTime < requiredData)
            {
                Runtime.Notify("Auction is expired");
                Storage.Delete(Storage.CurrentContext, key);
                return new BigInteger(0).AsByteArray();
            }

            Runtime.Notify("Price of Item", mItem.Price);

            // City, which on market Item was sold information
            string cityKey = CITY_PREFIX + mItem.City;
            byte[] lord = Storage.Get(Storage.CurrentContext, cityKey);

            // Calculate Price
            BigInteger percent = mItem.Price / 100;

            BigInteger ownerReceive = percent * auctionFee;
            BigInteger lordReceive = percent * lordFee;
            BigInteger sellerReceive = mItem.Price - (ownerReceive + lordReceive);

            bool ownerReceived = false;
            bool lordReceived = true;
            bool sellerReceived = false;

            Runtime.Notify("Owner should Receive", ownerReceive);
            Runtime.Notify("Lord should Receive", lordReceive);
            Runtime.Notify("Seller should Receive", sellerReceive, "Check income", "Seller", mItem.Seller);

            Transaction TX = (Transaction)Neo.SmartContract.Framework.Services.System.ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            foreach (var item in outputs)
            {
                Runtime.Notify("Item Attachment", item.ScriptHash, "Seller", mItem.Seller, "for amount", item.Value);
                // Seller of Item received money?
                if (item.ScriptHash.AsBigInteger() == mItem.Seller.AsBigInteger())
                {
                    Runtime.Notify("Seller received ", item.Value, " Gas! While required ", sellerReceive);
                    if (item.Value == sellerReceive)
                    {
                        sellerReceived = true;
                    }
                }

                // Game Developers got their fee?
                if (item.ScriptHash.AsBigInteger() == GameOwner.AsBigInteger())
                {
                    Runtime.Notify("Game Owner received ", item.Value, " Gas! While required ", ownerReceive);
                    if (item.Value == ownerReceive)
                    {
                        ownerReceived = true;
                    }
                }
                
                if (lord.Length == 0)
                {
                    lordReceived = true;
                } else if (item.ScriptHash.AsBigInteger() == lord.AsBigInteger())
                {
                    Runtime.Notify("City Lord received ", item.Value, " Gas! While required ", lordReceive);
                    if (new BigInteger(item.Value) == lordReceive)
                    {
                        lordReceived = true;
                    }
                }
            }

            if (ownerReceived && lordReceived && sellerReceived)
            {
                Runtime.Notify("Auction is expired");
                Storage.Delete(Storage.CurrentContext, key);

                // Change Item's owner too
                key = ITEM_PREFIX + itemId.AsByteArray();
                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

                item.OWNER = Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash;

                byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);
                Storage.Put(Storage.CurrentContext, key, itemBytes);

                Runtime.Notify("Item was successfully transferred to a new hero");
                return new BigInteger(1).AsByteArray();
            }
            // Get City Information
            // If there are not decided city data, set default Fee for lord tax
            // Check whether part of money send to the city
            // Check whether part of money send to the game owner
            // Check part of money send to item seller
            // If everything is OK, transfer Item to a new player

            Runtime.Notify("Some Transaction Fees are not included, Check SELLER, OWNER, LORD receivings", sellerReceived, ownerReceived, lordReceived);
            return new BigInteger(0).AsByteArray();
        }

        public static byte[] AuctionCancel(BigInteger itemId)
        {
            string key = MARKET_PREFIX + itemId.AsByteArray();
            Storage.Delete(Storage.CurrentContext, key);

            Runtime.Notify("Item was successfully deleted from Auction!");
            return new BigInteger(1).AsByteArray();
        }

        //------------------------------------------------------------------------------------
        //
        // GAME OWNER
        //
        //------------------------------------------------------------------------------------

        public static byte[] PutItem( BigInteger itemId, byte givenFor, Item item )
        {
            // Put Item's Editable Data on Storage
            string key = ITEM_PREFIX + itemId.AsByteArray();
  
            byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

            Storage.Put(Storage.CurrentContext, key, itemBytes);


            // Update Giving Items Amount
            if (givenFor == HERO_CREATION_GIVEN)
            {
                byte[] currentItem = Storage.Get(Storage.CurrentContext, HERO_CREATION_KEY);
                
                key = HERO_CREATION_PREFIX + itemId.AsByteArray();
                Storage.Put(Storage.CurrentContext, key, currentItem); // Previous Added item is linked as previous item

                Storage.Put(Storage.CurrentContext, HERO_CREATION_KEY, itemId);
            }
            else
            {
                byte[] currentItem = Storage.Get(Storage.CurrentContext, ITEM_DROP_KEY);

                key = ITEM_DROP_PREFIX + itemId.AsByteArray();
                Storage.Put(Storage.CurrentContext, key, currentItem); // Previous Added item is linked as previous item

                Storage.Put(Storage.CurrentContext, ITEM_DROP_KEY, itemId);
            }

            Runtime.Notify("Item was successfully stored on storage");
            return new BigInteger(1).AsByteArray();
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

        //------------------------------------------------------------------------------------
        private static bool IsAuctionTransactionFeeIncluded ( BigInteger duration )
        {
            Transaction TX = (Transaction)Neo.SmartContract.Framework.Services.System.ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            foreach (var item in outputs)
            {
                //if (item.Value == auction8HoursFee)
                //{
                //    Runtime.Notify("There are 0.1m Fee");
                //}
                if (item.Value == 10_000_000)
                {
                    Runtime.Notify("There are 10 Millions of Gas", item.ScriptHash.AsString(), auction8HoursFee);
                }
                Runtime.Notify("Output is", item.Value);
            }
            if (duration == 8)
            {
                Runtime.Notify("Valid 8 hours!");
                return true;
            }
            if (duration == 12)
            {
                Runtime.Notify("Valid 12 hours!");
                return true;
            }
            if (duration == 24)
            {
                Runtime.Notify("Valid 24 hours!");
                return true;
            }

            Runtime.Notify("Invalid Duration time, all included fee will be counted as invalid!");
            return true;
        }

        private static bool IsEditableItemDataExist(BigInteger itemId)
        {
            return false;
        }

        private static bool IsValidItemOwner ( BigInteger itemId, byte[] Owner )
        {
            return true;
        }
    }
}
