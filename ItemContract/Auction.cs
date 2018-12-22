using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

namespace LordsContract
{
    //------------------------------------------------------------------------------------
    //
    // functions for:
    // AUCTION
    //
    //------------------------------------------------------------------------------------
    public static class Auction {
        /**
         * Function records item addition onto the market.
         * 
         * Item Seller (the invoker of this function) should send some GAS as a fee to game owner.
         * It is checked inside of function
         * 
         * Has 5 arguments
         * @Item ID (BigInteger)                - ID of item that will be added onto the market.
         * @Auction Duration (BigInteger)       - Duration on market, amount of time that item placed on market.
         * @Price (BigInteger)                  - Fixed Price in GAS for Item, defined by Seller
         * @City (BigInteger)                   - ID of city, where Item was added on that cities market.
         * @Seller (byte[])                     - Wallet Address of Item Owner
         */
        public static byte[] Begin(BigInteger itemId, MarketItemData item)
        {
            // Check whether transaction fee is included?
            //if (!IsAuctionTransactionFeeIncluded(item.AuctionDuration))
            //{
            //    Runtime.Notify("Error! Transaction fee is not included!");
            //    return new BigInteger(0).AsByteArray();
            //}

            // TODO: Validate Item.

            string key = GeneralContract.MARKET_PREFIX + itemId.AsByteArray();

            // Serialize Custom Object `Item` into bytes, since Neo Storage doesn't support custom classes.
            byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

            // Save on Storage!!!
            Storage.Put(Storage.CurrentContext, key, itemBytes);

            return new BigInteger(1).AsByteArray();
        }

        /**
         * Function Records Item buying on Market and finishes Auction for Item.
         * 
         * It is Free from Transaction Fee.
         * 
         * Has 2 arguments
         * @Item ID (BigInteger)                - ID of item that should be removed onto the market.
         * @Buyer (byte[])                      - Wallet address of Item Buyer
         */
        public static byte[] End(BigInteger itemId, byte[] buyer)
        {
            // TODO Verify

            // Item Data that was on Market
            string key = GeneralContract.MARKET_PREFIX + itemId.AsByteArray();
            MarketItemData mItem = (MarketItemData)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            // Calculate the Valid Data
            BigInteger validStartedTime = Blockchain.GetBlock(Blockchain.GetHeight()).Timestamp - (mItem.AuctionDuration * 3600);   // Current Time - Auction Duration
            if (mItem.AuctionStartedTime < validStartedTime)
            {
                Runtime.Notify("Auction expired");
                Storage.Delete(Storage.CurrentContext, key);    // Remove expired Data from Market.
                return new BigInteger(0).AsByteArray();
            }

            // On Blockchain Storage, city stores Wallet Address of that city's owner.
            string cityKey = GeneralContract.CITY_PREFIX + mItem.City;

            City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, cityKey));

            byte[] lord = new byte[] { };
            if (city.Hero == 0)
                lord = GeneralContract.GameOwner;
            else
            {
                string heroKey = GeneralContract.HERO_PREFIX + city.Hero;
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, heroKey));

                lord = hero.OWNER;     // Owner of city, where item was sold
            }

            // Invocation of this functions comes with an attached GAS.
            // The total amount of attached GAS should be equal to the price of Item on Market.
            // There should 3 attachments. 
            // Attachment #1. GAS amount of 5 percents of Item Price and goes to City Owner
            // Attachment #2. GAS amount of 5 percents of Item Price and goes to Game Owner
            // Attachment #3. GAS amount of 90 percents of Item Price and goes to Item Seller.
            BigInteger percent = mItem.Price / 100;

            BigInteger ownerReceive = percent * GeneralContract.auctionFee;
            BigInteger lordReceive = percent * GeneralContract.lordFee;
            BigInteger sellerReceive = mItem.Price - (ownerReceive + lordReceive);

            bool ownerReceived = false;
            bool lordReceived = true;
            bool sellerReceived = false;

            Runtime.Notify("Owner should Receive", ownerReceive);
            Runtime.Notify("Lord should Receive", lordReceive);
            Runtime.Notify("Seller should Receive", sellerReceive, "Check income", "Seller", mItem.Seller);

            // Check Attachments that were included with current Transaction
            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            foreach (var item in outputs)
            {
                // Seller of Item received money?
                if (item.ScriptHash.AsBigInteger() == mItem.Seller.AsBigInteger())
                {
                    Runtime.Notify("Seller received ", item.Value, " Gas! While required ", sellerReceive);
                    if (item.Value == sellerReceive)
                    {
                        sellerReceived = true;
                        continue;
                    }
                }

                // Game Developers got their fee?
                if (item.ScriptHash.AsBigInteger() == GeneralContract.GameOwner.AsBigInteger())
                {
                    Runtime.Notify("Game Owner received ", item.Value, " Gas! While required ", ownerReceive);
                    if (item.Value == ownerReceive)
                    {
                        ownerReceived = true;
                        continue;
                    }
                }

                if (lord.Length == 0)
                {
                    lordReceived = true;
                }
                else if (item.ScriptHash.AsBigInteger() == lord.AsBigInteger())
                {
                    Runtime.Notify("City Lord received ", item.Value, " Gas! While required ", lordReceive);
                    if (new BigInteger(item.Value) == lordReceive)
                    {
                        lordReceived = true;
                        continue;
                    }
                }
            }

            if (ownerReceived && lordReceived && sellerReceived)
            {
                // Remove Item from Market.
                Storage.Delete(Storage.CurrentContext, key);

                // Change Item's owner too.
                key = GeneralContract.ITEM_PREFIX + itemId.AsByteArray();
                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

                item.OWNER = buyer;

                byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);
                Storage.Put(Storage.CurrentContext, key, itemBytes);

                Runtime.Notify("Item was successfully transferred to a new owner");
                return new BigInteger(1).AsByteArray();
            }

            Runtime.Notify("Some Transaction Fees are not included, Check SELLER, OWNER, LORD receivings", sellerReceived, ownerReceived, lordReceived);
            return new BigInteger(0).AsByteArray();
        }

        public static byte[] Cancel(BigInteger itemId)
        {
            Runtime.Log("Initialized Auction Cancellation");
            string key = GeneralContract.MARKET_PREFIX + itemId.AsByteArray();
            MarketItemData mItem = (MarketItemData)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

            if (Runtime.CheckWitness(mItem.Seller))
            {
                Runtime.Notify("Only Owner of Item can delete it from Market!");
                return new BigInteger(0).AsByteArray();
            }

            Storage.Delete(Storage.CurrentContext, key);

            Runtime.Notify("Item was successfully cancelled from Auction!");
            return new BigInteger(1).AsByteArray();
        }
    }

}


