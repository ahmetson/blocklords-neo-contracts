using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

namespace LordsContract
{
    public class Contract1 : SmartContract
    {
        private static readonly string MARKET_PREFIX = "\x01\x00";
        private static readonly string ITEM_PREFIX = "\x02\x00";
        private static readonly string STRONGHOLD_PREFIX = "\x03\x00";
        private static readonly string ITEM_DROP_PREFIX = "\x04\x00";
        private static readonly string BATTLE_LOG_PREFIX = "\x05\x00";
        private static readonly string ITEM_DROP_KEY = "\x06";
        private static readonly string CITY_PREFIX = "\x07\x00";
        private static readonly string HERO_PREFIX = "\x08\x00";

        private static readonly byte HERO_CREATION_GIVEN = 0;
        private static readonly byte ITEM_DROP_GIVEN = 1;

        private static readonly BigInteger auctionFee = 5;  // In percents amount of GAS that buyers sends to Game Developers for Auction
        private static readonly BigInteger lordFee = 5;     // In percents default amount of GAS that buyers sends to City Lords for Auction 

        private static readonly BigInteger DropInterval = 120; // Item will be dropped at each 120 blocks

        /**
         * 1 GAS === 100_000_000
         * 0.1GAS == 10_000_000
         */
        private static readonly decimal auction8HoursFee = 10_000_000,
                                           auction12HoursFee = 20_000_000,
                                           auction24HoursFee = 30_000_000,
                                           heroCreationFee = 100_000_000,
                                           cityAttackFee = 50_000_000,
            strongholdAttackFee = 20_000_000,
            banditCampAttackFee = 10_000_000;
                                     
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
            public BigInteger XP;
            public byte[] OWNER;
        }

        [Serializable]
        public class DropData
        {
            public BigInteger Block;
            public BigInteger StrongholdId;
            public BigInteger ItemId;
            public BigInteger HeroId;
        }

        [Serializable]
        public class City
        {
            public byte[] Owner;
            public BigInteger TaxPercents;
        }

        [Serializable]
        public class Hero
        {
            public byte[] OWNER;
            public BigInteger TROOPS_CAP;
            public BigInteger LEADERSHIP;
            public BigInteger INTELLIGENCE;
            public BigInteger STRENGTH;
            public BigInteger SPEED;
            public BigInteger DEFENSE;
            public byte[] TX;
        }

        [Serializable]
        public class Stronghold
        {
            public BigInteger ID;
            public BigInteger Hero;
            public BigInteger CreatedBlock;
        }

        public static BigInteger GetRandomNumber(ulong max = 10)
        {
            //Transaction tx = (Transaction)ExecutionEngine.ScriptContainer;
            //Header bl = Blockchain.GetHeader(Blockchain.GetHeight());
            //byte[] h = bl.Hash.Concat(tx.Hash);
            //var hash = Hash256(h);
            //byte[] rand = hash.Range(0, size_in_bytes);
            //return rand.AsBigInteger();

            Header header = Blockchain.GetHeader(Blockchain.GetHeight());
            ulong randomNumber = header.ConsensusData;
            int percentage = (int)(randomNumber % max);

            return new BigInteger(percentage);
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
            }
            else if (param.Equals("putItem"))
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
                item.OWNER = ExecutionEngine.CallingScriptHash;
                item.XP = 0;

                // Method puts item on storage.
                // Updates Item Given Parameters
                PutItem((BigInteger)args[1], (byte)args[0], item);
            }
            else if (param.Equals("putHero"))
            {
                if (args.Length != 13)
                {
                    Runtime.Log("Invalid parameters");
                    return new BigInteger(0).AsByteArray();
                }

                Runtime.Log("Hero putting initialized");
                Hero hero = new Hero();
                hero.OWNER = (byte[])args[1];
                hero.TROOPS_CAP = (BigInteger)args[2];
                hero.INTELLIGENCE = (BigInteger)args[3];
                hero.SPEED = (BigInteger)args[4];
                hero.STRENGTH = (BigInteger)args[5];
                hero.LEADERSHIP = (BigInteger)args[6];
                hero.DEFENSE = (BigInteger)args[7];
                hero.TX = ((Transaction)Neo.SmartContract.Framework.Services.System.ExecutionEngine.ScriptContainer).Hash;

                return PutHero((BigInteger)args[0], hero, (BigInteger)args[8], (BigInteger)args[9], (BigInteger)args[10], (BigInteger)args[11], (BigInteger)args[12]);
            }
            else if (param.Equals("auctionEnd"))
            {
                // Check Does item exist
                Runtime.Log("Calling Auction End");

                //return new BigInteger(0).AsByteArray();
                return AuctionEnd((BigInteger)args[0]);
            }
            else if (param.Equals("dropItems"))
            {
                // Check Witnesses
                if (!Runtime.CheckWitness(GameOwner))
                {
                    Runtime.Log("Permission denied! Only game admin can add new items. Atleast for now!");
                    return new BigInteger(0).AsByteArray();
                }

                return DropItems();
            }
            else if (param.Equals("LogCityAttack"))
            {
                Runtime.Log("Initialize city attack");
                return LogCityAttack(args);
            }
            else if (param.Equals("LogStrongholdAttack"))
            {
                Runtime.Log("Initialize stronghold attack");
                return LogStrongholdAttack(args);
            }
            else if (param.Equals("LogBanditCampAttack"))
            {
                Runtime.Log("Initialize bandir camp attack");
                return LogBanditCampAttack(args);
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
            /* Since, which Items will be given to hero are decided by Server side, we delete Item Giving Manager from SmartContract for Hero Creation
             * if (givenFor == HERO_CREATION_GIVEN)
            {
                byte[] currentItem = Storage.Get(Storage.CurrentContext, HERO_CREATION_KEY);
                
                key = HERO_CREATION_PREFIX + itemId.AsByteArray();
                Storage.Put(Storage.CurrentContext, key, currentItem); // Previous Added item is linked as previous item

                Storage.Put(Storage.CurrentContext, HERO_CREATION_KEY, itemId);
            }
            else*/ if (givenFor == ITEM_DROP_GIVEN)
            {
                byte[] currentItem = Storage.Get(Storage.CurrentContext, ITEM_DROP_KEY);

                key = ITEM_DROP_PREFIX + itemId.AsByteArray();
                Storage.Put(Storage.CurrentContext, key, currentItem); // Previous Added item is linked as previous item

                Storage.Put(Storage.CurrentContext, ITEM_DROP_KEY, itemId);
            }

            Runtime.Notify("Item was successfully stored on storage");
            return new BigInteger(1).AsByteArray();
        }

        private static byte[] PutHero(BigInteger heroId, Hero hero, BigInteger item1, BigInteger item2, BigInteger item3, BigInteger item4, BigInteger item5)
        {
            // Check Transaction Fee
            bool received = false;
            Transaction TX = (Transaction)Neo.SmartContract.Framework.Services.System.ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            foreach (var output in outputs)
            {

                // Game Developers got their fee?
                if (output.ScriptHash.AsBigInteger() == GameOwner.AsBigInteger())
                {
                    if (output.Value == heroCreationFee)
                    {
                        received = true;
                        break;
                    }
                }
            }

            if (!received)
            {
                Runtime.Notify("Hero Creation Fee is not included! Hero was not stored on Blockchain");
                return new BigInteger(0).AsByteArray();
            }

            // Put Hero
            string key = HERO_PREFIX + heroId.AsByteArray();

            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(hero);

            Storage.Put(Storage.CurrentContext, key, bytes);

            // Give Item #1 to Created Hero
            key = ITEM_PREFIX + item1.AsByteArray();
            Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            item.OWNER = hero.OWNER;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            // Give Item #2 to Created Hero
            key = ITEM_PREFIX + item2.AsByteArray();
            item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            item.OWNER = hero.OWNER;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            // Give Item #3 to Created Hero
            key = ITEM_PREFIX + item3.AsByteArray();
            item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            item.OWNER = hero.OWNER;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            // Give Item #4 to Created Hero
            key = ITEM_PREFIX + item4.AsByteArray();
            item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            item.OWNER = hero.OWNER;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            // Give Item #5 to Created Hero
            key = ITEM_PREFIX + item5.AsByteArray();
            item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            item.OWNER = hero.OWNER;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            Runtime.Log("Hero was created and Hero got his Items");

            return new BigInteger(1).AsByteArray();
        }

        //------------------------------------------------------------------------------------
        //
        // ITEM DROPS
        //
        //------------------------------------------------------------------------------------

        public static byte[] DropItems()
        {
            // Check the Validness of the time
            DropData dropData = (DropData)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, ITEM_DROP_KEY));

            BigInteger currentBlock = Blockchain.GetHeight();

            if (dropData != null && currentBlock < dropData.Block + DropInterval)
            {
                Runtime.Notify("To early to drop Items! Last Block", dropData.Block, "Current Block", currentBlock);
                return new BigInteger(0).AsByteArray();  
            }

            BigInteger totalWeight = 0;
            BigInteger[] mins = new BigInteger[10];
            BigInteger[] maxes = new BigInteger[10];

            string key = "";
            Stronghold inStronghold;

            for (int i=0; i<10; i++)
            {
                key = STRONGHOLD_PREFIX + (i + 1).Serialize();

                byte[] inStrongholdBytes = Storage.Get(Storage.CurrentContext, key);

                if (inStrongholdBytes.Length == 0)
                {
                    mins[i] = maxes[i] = 0;
                } else
                {
                    inStronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(inStrongholdBytes);

                    if (i == 0)
                        mins[i] = 0;
                    else
                        mins[i] = maxes[i - 1];

                    BigInteger weight = Blockchain.GetHeight() - inStronghold.CreatedBlock;

                    maxes[i] = mins[i] + weight;

                    totalWeight += weight;
                }
                
                
            }

            if (totalWeight == 0)
            {
                Runtime.Notify("There no Stronghold owners");
                return new BigInteger(0).AsByteArray();
            }
            else
            {
                BigInteger randomWeight = GetRandomNumber((ulong)totalWeight-1) + 1;
                Runtime.Notify("Returned Random Stronghold Weight", randomWeight, "Define Stronghold ID");

                // Find Stronghold based on Weight
                for(var i=0; i<10; i++)
                {
                    if (randomWeight >= mins[i] && randomWeight <= maxes[i])    //TODOOOOOOOOOOOOOOOOOOOOOOOOOOO if randomWeight is equal to min or max, this logic command will return FALSE?
                    {
                        // Is Stronghold has an owner?
                        key = STRONGHOLD_PREFIX + (i + 1).Serialize();
                        break;
                    }
                }

                

                byte[] inStrongholdBytes = Storage.Get(Storage.CurrentContext, key);
                inStronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(inStrongholdBytes);

                if (inStronghold.Hero <= 0)
                {
                    Runtime.Notify("Stronghold is owned by NPC");
                    return new BigInteger(0).AsByteArray();
                }

                // Check existence of Drop Item
                var nextDropItem = Storage.Get(Storage.CurrentContext, ITEM_DROP_KEY);
                if (nextDropItem.Length == 0)
                {
                    Runtime.Notify("Item to drop doesn't exists");
                    return new BigInteger(0).AsByteArray();
                }

                // If Drop Item exists get item id from it
                string itemKey = ITEM_PREFIX + nextDropItem;
                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, itemKey));

                string heroKey = HERO_PREFIX + inStronghold;
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, heroKey));


                // Change owner of Item.
                item.OWNER = hero.OWNER;
                byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

                // Change Drop Item.
                Storage.Put(Storage.CurrentContext, itemKey, itemBytes);

                // Delete Stronghold owner. (It means "kicking out from Stronghold")
                inStronghold.Hero = 0;
                inStrongholdBytes = Neo.SmartContract.Framework.Helper.Serialize(inStronghold);
                Storage.Put(Storage.CurrentContext, key, inStrongholdBytes);

                key = ITEM_DROP_PREFIX + nextDropItem;
                BigInteger previousItem = new BigInteger(Storage.Get(Storage.CurrentContext, key)); // Previous Added item is linked as previous item

                // Set the drop item
                Storage.Delete(Storage.CurrentContext, key);
                Storage.Put(Storage.CurrentContext, ITEM_DROP_KEY, previousItem); // Set Prvious Drop item as next droppable item


                // Set Stronghold update time.
                DropData dropped = new DropData();
                dropped.Block = Blockchain.GetHeight();
                dropped.HeroId = inStronghold.Hero;
                dropped.ItemId = nextDropItem.AsBigInteger();
                dropped.StrongholdId = inStronghold.ID;

                byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(dropped);

                Storage.Put(Storage.CurrentContext, ITEM_DROP_KEY, bytes);
            }

            Runtime.Notify("Item was Dropped successfully");
            return new BigInteger(1).AsByteArray();
        }

        //------------------------------------------------------------------------------------
        //
        // Item Edit
        //
        //------------------------------------------------------------------------------------

        public static BigInteger[] UpdateItemStats(BigInteger[] ids)
        {
            Runtime.Notify("Init Item Stat Update");

            string key = "";

            BigInteger[] updateValues = new BigInteger[5] { 0,0,0,0,0 };

            for(var i=0; i<5; i++)
            {
                // Get Item
                key = ITEM_PREFIX + ids[i].AsByteArray();
                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

                // Update XP
                item.XP = item.XP + 1;

                // Update Level
                if (item.QUALITY == 1 && item.LEVEL == 3 ||
                    item.QUALITY == 2 && item.LEVEL == 5 ||
                    item.QUALITY == 3 && item.LEVEL == 7 ||
                    item.QUALITY == 4 && item.LEVEL == 9 ||
                    item.QUALITY == 5 && item.LEVEL == 10)
                {
                    Runtime.Notify("The Item had reached max level. So not updated", ids[i]);
                    updateValues[i] = 0;
                    continue;
                }

                if (item.LEVEL == 1 && item.XP >= 4 ||
                    item.LEVEL == 2 && item.XP >= 14 ||
                    item.LEVEL == 3 && item.XP >= 34 ||
                    item.LEVEL == 4 && item.XP >= 74 ||
                    item.LEVEL == 5 && item.XP >= 144 ||
                    item.LEVEL == 6 && item.XP >= 254 ||
                    item.LEVEL == 7 && item.XP >= 404 ||
                    item.LEVEL == 8 && item.XP >= 604 ||
                    item.LEVEL == 9 && item.XP >= 904)
                {
                    item.LEVEL = item.LEVEL + 1;
                }

                // Update Stat based on level
                if (item.QUALITY == 1)
                {
                    updateValues[i] = GetRandomNumber(3);
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                } else if (item.QUALITY == 2)
                {
                    updateValues[i] = GetRandomNumber(3) + 2;
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                else if (item.QUALITY == 3)
                {
                    updateValues[i] = GetRandomNumber(3) + 4;
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                else if (item.QUALITY == 4)
                {
                    updateValues[i] = GetRandomNumber(3) + 6;
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                else if(item.QUALITY == 5)
                {
                    updateValues[i] = GetRandomNumber(3) + 8;
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                /*else if (item.LEVEL == 6)
                {
                    updateValues[i] = GetRandomNumber(3);
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                else if(item.LEVEL == 7)
                {
                    updateValues[i] = GetRandomNumber(3);
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                else if (item.LEVEL == 8)
                {
                    updateValues[i] = GetRandomNumber(3);
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                else if(item.LEVEL == 9)
                {
                    updateValues[i] = GetRandomNumber(3);
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                else 
                {
                    updateValues[i] = GetRandomNumber(3);
                    item.STAT_VALUE = item.STAT_VALUE + updateValues[i];
                }
                */


                // Record generated Stat value on Update Values List
                byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(item);


                // Put Item Back
                Storage.Put(Storage.CurrentContext, key, bytes);
            }

            return updateValues;
        }

        //------------------------------------------------------------------------------------
        //
        // LOG
        //
        //------------------------------------------------------------------------------------
        [Serializable]
        public class BattleLog
        {
            public BigInteger BattleId;
            public BigInteger BattleResult; // 0 - Attacker WON, 1 - Attacker Lose
            public BigInteger BattleType;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            public BigInteger Attacker;
            public byte[] AttackerOwner;
            public BigInteger AttackerTroops;
            public BigInteger AttackerRemained;
            public BigInteger AttackerItem1;
            public BigInteger AttackerItem2;
            public BigInteger AttackerItem3;
            public BigInteger AttackerItem4;
            public BigInteger AttackerItem5;
            public BigInteger DefenderObject;   // City|Stronghold|NPC ID

            public BigInteger Defender;
            public byte[] DefenderOwner;
            public BigInteger DefenderTroops;
            public BigInteger DefenderRemained;
            public BigInteger DefenderItem1;
            public BigInteger DefenderItem2;
            public BigInteger DefenderItem3;
            public BigInteger DefenderItem4;
            public BigInteger DefenderItem5;

            public BigInteger Time;
            public byte[] TX;
        }
        public static byte[] LogCityAttack(object[] args)
        {
            // Check incoming fee
            bool received = false;
            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            foreach (var output in outputs)
            {
                // Game Developers got their fee?
                if (output.ScriptHash.AsBigInteger() == GameOwner.AsBigInteger())
                {
                    Runtime.Notify("Game Owner received ", output.Value);
                    if (output.Value == cityAttackFee)
                    {
                        received = true;
                        break;
                    }
                }
            }

            if (!received)
            {
                Runtime.Notify("The Battle Fee doesn't included.");
                return new BigInteger(0).AsByteArray();
            }

            // Prepare log
            BattleLog log = new BattleLog();

            log.BattleId = (BigInteger)args[0];
            log.BattleResult = (BigInteger)args[1]; // 0 - Attacker WON, 1 - Attacker Lose
            log.BattleType = 1;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            log.Attacker = (BigInteger)args[2]; // Hero
            log.AttackerOwner = (byte[])args[3];    // Player Address
            log.AttackerTroops = (BigInteger)args[4];
            log.AttackerRemained = (BigInteger)args[5];
            log.AttackerItem1 = (BigInteger)args[6];    // Equipped Items that were involved
            log.AttackerItem2 = (BigInteger)args[7];
            log.AttackerItem3 = (BigInteger)args[8];
            log.AttackerItem4 = (BigInteger)args[9];
            log.AttackerItem5 = (BigInteger)args[10];
            log.DefenderObject = (BigInteger)args[20];   // City|Stronghold|NPC ID

            log.Defender = (BigInteger)args[11];
            log.DefenderOwner = (byte[])args[12];
            log.DefenderTroops = (BigInteger)args[13];
            log.DefenderRemained = (BigInteger)args[14];
            log.DefenderItem1 = (BigInteger)args[15];
            log.DefenderItem2 = (BigInteger)args[16];
            log.DefenderItem3 = (BigInteger)args[17];
            log.DefenderItem4 = (BigInteger)args[18];
            log.DefenderItem5 = (BigInteger)args[19];

            log.Time = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;
            log.TX = ((Transaction)ExecutionEngine.ScriptContainer).Hash;

            // Log 
            string key = BATTLE_LOG_PREFIX + log.TX;

            //item.Seller = Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash;

            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(log);

            Storage.Put(Storage.CurrentContext, key, bytes);

            // Change City Lord
            key = CITY_PREFIX + log.DefenderObject.AsByteArray();
            if (log.BattleResult == 1)  // Attacker Won?
                Storage.Put(Storage.CurrentContext, key, log.Attacker);
            //else
            //    Storage.Put(Storage.CurrentContext, key, log.Defender);

            Runtime.Notify("City Attack was logged on Blockchain");
            return new BigInteger(1).AsByteArray();
        }

        public static byte[] LogStrongholdAttack(object[] args)
        {
            // Check incoming fee
            bool received = false;
            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            foreach (var output in outputs)
            {
                // Game Developers got their fee?
                if (output.ScriptHash.AsBigInteger() == GameOwner.AsBigInteger())
                {
                    Runtime.Notify("Game Owner received ", output.Value);
                    if (output.Value == strongholdAttackFee)
                    {
                        received = true;
                        break;
                    }
                }
            }

            if (!received)
            {
                Runtime.Notify("The Battle Fee doesn't included.");
                return new BigInteger(0).AsByteArray();
            }

            // Prepare log
            BattleLog log = new BattleLog();

            log.BattleId = (BigInteger)args[0];
            log.BattleResult = (BigInteger)args[1]; // 0 - Attacker WON, 1 - Attacker Lose
            log.BattleType = 2;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            log.Attacker = (BigInteger)args[2]; // Hero
            log.AttackerOwner = (byte[])args[3];    // Player Address
            log.AttackerTroops = (BigInteger)args[4];
            log.AttackerRemained = (BigInteger)args[5];
            log.AttackerItem1 = (BigInteger)args[6];    // Equipped Items that were involved
            log.AttackerItem2 = (BigInteger)args[7];
            log.AttackerItem3 = (BigInteger)args[8];
            log.AttackerItem4 = (BigInteger)args[9];
            log.AttackerItem5 = (BigInteger)args[10];
            log.DefenderObject = (BigInteger)args[20];   // City|Stronghold|NPC ID

            log.Defender = (BigInteger)args[11];
            log.DefenderOwner = (byte[])args[12];
            log.DefenderTroops = (BigInteger)args[13];
            log.DefenderRemained = (BigInteger)args[14];
            log.DefenderItem1 = (BigInteger)args[15];
            log.DefenderItem2 = (BigInteger)args[16];
            log.DefenderItem3 = (BigInteger)args[17];
            log.DefenderItem4 = (BigInteger)args[18];
            log.DefenderItem5 = (BigInteger)args[19];

            log.Time = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;
            log.TX = ((Transaction)ExecutionEngine.ScriptContainer).Hash;

            // Log 
            string key = BATTLE_LOG_PREFIX + log.TX;

            //item.Seller = Neo.SmartContract.Framework.Services.System.ExecutionEngine.CallingScriptHash;

            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(log);

            Storage.Put(Storage.CurrentContext, key, bytes);

            // Change City Lord
            key = STRONGHOLD_PREFIX + log.DefenderObject.AsByteArray();
            if (log.BattleResult == 1) // Attacker Won?
            {
                Stronghold stronghold = new Stronghold();
                stronghold.CreatedBlock = Blockchain.GetHeight();
                stronghold.ID = log.DefenderObject;
                stronghold.Hero = log.Attacker;

                bytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);

                Storage.Put(Storage.CurrentContext, key, bytes);
            }

            Runtime.Notify("Stronghold Attack was logged on Blockchain");
            return new BigInteger(1).AsByteArray();
        }

        public static byte[] LogBanditCampAttack(object[] args)
        {
            // Check incoming fee
            bool received = false;
            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            foreach (var output in outputs)
            {
                // Game Developers got their fee?
                if (output.ScriptHash.AsBigInteger() == GameOwner.AsBigInteger())
                {
                    Runtime.Notify("Game Owner received ", output.Value);
                    if (output.Value == banditCampAttackFee)
                    {
                        received = true;
                        break;
                    }
                }
            }

            if (!received)
            {
                Runtime.Notify("The Battle Fee doesn't included.");
                return new BigInteger(0).AsByteArray();
            }

            // Prepare log
            BattleLog log = new BattleLog();

            log.BattleId = (BigInteger)args[0];
            log.BattleResult = (BigInteger)args[1]; // 0 - Attacker WON, 1 - Attacker Lose
            log.BattleType = 3;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
            log.Attacker = (BigInteger)args[2]; // Hero
            log.AttackerOwner = (byte[])args[3];    // Player Address
            log.AttackerTroops = (BigInteger)args[4];
            log.AttackerRemained = (BigInteger)args[5];
            log.AttackerItem1 = (BigInteger)args[6];    // Equipped Items that were involved
            log.AttackerItem2 = (BigInteger)args[7];
            log.AttackerItem3 = (BigInteger)args[8];
            log.AttackerItem4 = (BigInteger)args[9];
            log.AttackerItem5 = (BigInteger)args[10];
            log.DefenderObject = (BigInteger)args[11];   // City|Stronghold|NPC ID

            // No need to record NPC data!!!

            log.Time = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;
            log.TX = ((Transaction)ExecutionEngine.ScriptContainer).Hash;

            // Update Involved Items in Battle???
            //if (log.BattleResult == 1) // Attacker Won?
            //{
                BigInteger[] ids = new BigInteger[5]
                {
                    log.AttackerItem1,
                    log.AttackerItem2,
                    log.AttackerItem3,
                    log.AttackerItem4,
                    log.AttackerItem5
                };
                BigInteger[] stats = UpdateItemStats(ids);

                // Instead we use Defender Items List to records Hero's Item Update Values
                log.DefenderItem1 = stats[0];
                log.DefenderItem2 = stats[1];
                log.DefenderItem3 = stats[2];
                log.DefenderItem4 = stats[3];
                log.DefenderItem5 = stats[4];
            //}
            // Log 
            string key = BATTLE_LOG_PREFIX + log.TX;

            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(log);

            Storage.Put(Storage.CurrentContext, key, bytes);


            Runtime.Notify("Bandit Camp Attack was logged on Blockchain");
            return new BigInteger(1).AsByteArray();
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
