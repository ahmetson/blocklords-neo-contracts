using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

namespace LordsContract
{
    public static class Put
    {
        //------------------------------------------------------------------------------------
        //
        // functions for:
        // GAME OBJECT CREATION
        //
        //------------------------------------------------------------------------------------

        /**
         * Function puts item data onto the Blockchain Storage.
         * 
         * This function is only invocable by Game Developer.
         * 
         * Has 7 arguments:
         * @Item ID (BigInteger)    - ID of Item that will be added to the Storage
         * @Given For (byte)        - Item added into the batch. And will be given to game players as a reward. For what it can be given?
         * @Stat Type (byte)        - Item Parameter. Check Item Struct for more info.
         * @Quality (byte)          - Item Parameter. Check Item Struct for more info.
         * @Generation (BigInteger) - Batch Series number. Check Item Struct for more info.
         * @Stat Value (BigInteger) - Item Parameter. Check Item Struct for more info.
         * @Level (BigInteger)      - Item Parameter. Check Item Struct for more info.
         */
        public static byte[] Item(BigInteger itemId, byte givenFor, Item item)
        {
            // Put Item's Editable Data on Storage
            string key = GeneralContract.ITEM_PREFIX + itemId.AsByteArray();

            byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

            Storage.Put(Storage.CurrentContext, key, itemBytes);

            if (givenFor == GeneralContract.STRONGHOLD_REWARD)
            {
                // We have an items batch to reward for stronghold owning.
                // But, invocation of list of reward items at once, will be very expensive in Smartcontract, so we use another technique:
                // We store 2 kind of parameters on Blockchain storage.
                //      1) ID of Next Item for reward.
                //      2) Each Item for reward with a link to previous item for reward. ( key=>value) or ( item id => previous Item id)

                byte[] nextItem = Storage.Get(Storage.CurrentContext, GeneralContract.NEXT_REWARD_ITEM_KEY);

                // Add Putted Item onto the Blockchain Storage with a link to previously added item. (See latest line on comment above)
                key = GeneralContract.STRONGHOLD_REWARD_ITEM_KEY_PREFIX + itemId.AsByteArray();
                Storage.Put(Storage.CurrentContext, key, nextItem); // Previous Added item is linked as previous item

                // Set putted item as a next for rewarding. 
                Storage.Put(Storage.CurrentContext, GeneralContract.NEXT_REWARD_ITEM_KEY, itemId);
            }

            Runtime.Notify("Item was successfully stored on storage");
            return new BigInteger(1).AsByteArray();
        }

        /**
             * Function puts hero data onto the Blockchain storage.
             * 
             * Hero Owner (the invoker of this function) should send some GAS as a fee to game owner.
             * It is checked inside of function
             * 
             * Has 13 arguments
             * @Hero ID (BigInteger)                - ID of hero that will be added onto the Blockchain Storage.
             * @Owner (byte[])                      - The wallet address of Hero Owner.
             * @Troops cap (BigInteger)             - Limit amount for troops
             * @Intelligence (BigInteger)           - Base stat
             * @Speed (BigInteger)                  - Another base stat
             * @Strength (BigInteger)               - Another base stat
             * @Leadership                          - base stat too
             * @Defense                             - last base stat
             * @Signature (byte[])
             * @Item ID #1 (BigInteger)             - Rewarded Item ID. (Hero is rewarded by 5 items when created.)
             * @Item ID #2
             * @Item ID #3
             * @Item ID #4
             * @Item ID #5
             * @Transaction                         - where fee was recorded
             */
        public static byte[] Hero(BigInteger heroId, Hero hero, byte[] signature)
        {
            string key = GeneralContract.ITEM_PREFIX + hero.Equipments[0].AsByteArray();
            // If Item's are not exist, exit
            byte[] itemBytes1 = Storage.Get(Storage.CurrentContext, key);
            if (itemBytes1.Length == 0)
            {
                Runtime.Notify("Item is not exists", hero.Equipments[0]);
                return new BigInteger(0).AsByteArray();
            }
            // TODO item should not be owned by anyone.
            key = GeneralContract.ITEM_PREFIX + hero.Equipments[1].AsByteArray();
            byte[] itemBytes2 = Storage.Get(Storage.CurrentContext, key);
            if (itemBytes2.Length == 0)
            {
                Runtime.Notify("Item is not exists", hero.Equipments[1]);
                return new BigInteger(0).AsByteArray();
            }
            key = GeneralContract.ITEM_PREFIX + hero.Equipments[2].AsByteArray();
            byte[] itemBytes3 = Storage.Get(Storage.CurrentContext, key);
            if (itemBytes3.Length == 0)
            {
                Runtime.Notify("Item is not exists", hero.Equipments[2]);
                return new BigInteger(0).AsByteArray();
            }
            key = GeneralContract.ITEM_PREFIX + hero.Equipments[3].AsByteArray();
            byte[] itemBytes4 = Storage.Get(Storage.CurrentContext, key);
            if (itemBytes4.Length == 0)
            {
                Runtime.Notify("Item is not exists", hero.Equipments[3]);
                return new BigInteger(0).AsByteArray();
            }
            key = GeneralContract.ITEM_PREFIX + hero.Equipments[4].AsByteArray();
            byte[] itemBytes5 = Storage.Get(Storage.CurrentContext, key);
            if (itemBytes5.Length == 0)
            {
                Runtime.Notify("Item is not exist", hero.Equipments[4]);
                return new BigInteger(0).AsByteArray();
            }

            // Give Item #1 to Created Hero, which means Change owner of Item to the Owner of Hero

            // Putting Hero costs 1 GAS for player.
            // Check attachments to Transaction, where should be sended 1 GAS to Game Owner
            bool received = false;
            Transaction tx = Blockchain.GetTransaction(hero.Fee_TX);
            TransactionOutput[] outputs = tx.GetOutputs();
            foreach (var output in outputs)
            {

                // Game Developers got their fee?
                if (output.ScriptHash.AsBigInteger() == GeneralContract.GameOwner.AsBigInteger())
                {
                    if (output.Value == GeneralContract.heroCreationFee)
                    {
                        received = true;
                        break;
                    }
                }
            }

            if (!received)
            {
                Runtime.Notify("Hero Creation Fee is not included! Hero wasn't putted on Blockchain");
                return new BigInteger(0).AsByteArray();
            }

            // Put Hero
            key = GeneralContract.HERO_PREFIX + heroId.AsByteArray();

            byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(hero);

            Storage.Put(Storage.CurrentContext, key, bytes);

            // Give Item #1 to Created Hero, which means Change owner of Item to the Owner of Hero
            Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes1);

            item.OWNER = hero.OWNER;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            // Give Item #2 to Created Hero
            item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes2);

            item.OWNER = hero.OWNER;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            // Give Item #3 to Created Hero
            item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes3);

            item.OWNER = hero.OWNER;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            // Give Item #4 to Created Hero
            item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes4);

            item.OWNER = hero.OWNER;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            // Give Item #5 to Created Hero
            item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes5);

            item.OWNER = hero.OWNER;
            bytes = Neo.SmartContract.Framework.Helper.Serialize(item);
            Storage.Put(Storage.CurrentContext, key, bytes);

            Runtime.Log("Hero was created and Hero got his Items");

            return new BigInteger(1).AsByteArray();
        }


        /// <summary>
        /// Method is invoked by Game Owner.
        /// 
        /// When This method is invoked, it should invlude transaction fee to Smartcontract address.
        /// </summary>
        /// <param name="args">List of City IDs and Sizes</param>
        /// <returns></returns>
        public static byte[] Cities(object[] args)
        {
            // First comes city ids, then sizes
            if (args.Length % 3 != 0)
            {
                Runtime.Notify("Invalid Parameters");
                return new BigInteger(0).AsByteArray();
            }

            // Invoker has permission to execute this function?
            if (!Runtime.CheckWitness(GeneralContract.GameOwner))
            {
                Runtime.Log("Permission denied! Only game admin can use this function!");
                return new BigInteger(0).AsByteArray();
            }

            Runtime.Notify("The Smartcontract address", ExecutionEngine.ExecutingScriptHash);

            // Define Required Price
            BigInteger price = 0;

            string key = "";
            int id = 0; int size = args.Length / 2;


            // Check all incoming items are for Smartcontract
            for (; id < args.Length / 2; id++, size++)
            {
                key = GeneralContract.CITY_PREFIX + ((BigInteger)args[id]).AsByteArray();
                byte[] cityBytes = Storage.Get(Storage.CurrentContext, key);
                if (cityBytes.Length != 0)
                {
                    Runtime.Notify("City already exists");
                    return new BigInteger(0).AsByteArray();
                }

                City city = new City();

                BigInteger sizeValue = (BigInteger)args[size];

                if (sizeValue == GeneralContract.bigCity)
                {
                    city.Coffer = new BigInteger(GeneralContract.bigCityCoffer.Serialize());

                }
                else if (sizeValue == GeneralContract.mediumCity)
                {
                    city.Coffer = new BigInteger(GeneralContract.mediumCityCoffer.Serialize());
                }
                else if (sizeValue == GeneralContract.smallCity)
                {
                    city.Coffer = new BigInteger(GeneralContract.smallCityCoffer.Serialize());
                }

                city.ID = (BigInteger)args[id];
                city.CreatedBlock = Blockchain.GetHeight();
                city.Size = sizeValue;
                city.Hero = 0;          // NPC owned

                cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);

                Storage.Put(Storage.CurrentContext, key, cityBytes);
            }


            /* Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
             TransactionOutput[] outputs = TX.GetOutputs();
             Runtime.Notify("Outputs are", outputs.Length);
             foreach (var item in outputs)
             {
                 Runtime.Notify("Output is", item.Value, "to", item.ScriptHash);
             }*/

            Runtime.Notify("City Information was added successfully");
            return new BigInteger(1).AsByteArray();
        }

    }
}


