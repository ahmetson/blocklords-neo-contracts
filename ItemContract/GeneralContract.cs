using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;
using System.Security.Cryptography;

namespace LordsContract
{
    public class GeneralContract : SmartContract
    {
        public static readonly string MARKET_PREFIX = "\x01\x00";
        public static readonly string ITEM_PREFIX = "\x02\x00";
        public static readonly string STRONGHOLD_PREFIX = "\x03\x00";
        public static readonly string STRONGHOLD_REWARD_ITEM_KEY_PREFIX = "\x04\x00";
        public static readonly string BATTLE_LOG_PREFIX = "\x05\x00";
        public static readonly string NEXT_REWARD_ITEM_KEY = "\x06";
        public static readonly string CITY_PREFIX = "\x07\x00";
        public static readonly string HERO_PREFIX = "\x08\x00";
        public static readonly string LATEST_REWARDED_ITEM_KEY = "\x09",
                                                                         COFFER_PREFIX = "\x10\x00",
                                                                         COFFER_PAYOUT_KEY = "\x11";

        // Items may be given to heroes in two situation: when they create hero or when they own some territory on the game map.
        public static readonly byte HERO_CREATION_GIVEN = 0;
        public static readonly byte STRONGHOLD_REWARD = 1;

        public static readonly BigInteger auctionFee = 5;  // In percents amount of GAS that buyers sends to Game Developers for Putting Item on Market
        public static readonly BigInteger lordFee = 5;     // In percents default amount of GAS that buyers sends to City Lords for Putting Item on Market

        private static readonly BigInteger DropInterval = 120; // Item will be dropped in every 120 blocks. On Neo Blockchain, each block is generated within 20-30 seconds.

        /**
         * 1 GAS === 100_000_000
         * 0.1GAS == 10_000_000
         * 
         * Neo Blockchain's Virtual Machine, where Smartcontracts are executed doesn't support Float numbers.
         * So all incoming Float numbers are converted and used in multiplication of 100_000_000.
         * 
         * Basically 0.1 means 10000000 (10_000_000) during Execution of Smartcontract.
         */
        public static readonly decimal auction8HoursFee = 10_000_000,              // 0.1 GAS
                                                                                    //auction12HoursFee = 20_000_000,          // 0.2 GAS
                                                                                    //auction24HoursFee = 30_000_000,          // 0.3 GAS
                                           heroCreationFee = 100_000_000,           // 1.0 GAS
                                           cityAttackFee = 50000000,              // 0.5 GAS
            strongholdAttackFee = 20_000_000,                                       // 0.2 GAS
            banditCampAttackFee = 10_000_000,                                       // 0.1 GAS
            bigCityCoffer = 100000000,                                      // 1.0 GAS
            mediumCityCoffer = 70000000,                                       // 0.7 GAS
            smallCityCoffer = 50000000;                                       // 0.5 GAS

        // Item can be on Market for 8, 12, 24 hours. If someone tries to buy item on market after expiration,
        // Then, buying item will be invalid.
        public static readonly BigInteger duration8Hours = 28800;                  // 28_800 Seconds are 8 hours
        public static readonly BigInteger duration12Hours = 43200;                 // 43_200 Seconds are 12 hours
        public static readonly BigInteger duration24Hours = 86400;                 // 86_400 Seconds are 24 hours

        // The Smartcontract Owner's Wallet Address. Used to receive some Gas as a transaction fee.
        public static readonly byte[] GameOwner = "AML8hyTV4vXuomovxdcAH9pRC9ny618YmA".ToScriptHash();

        public static readonly BigInteger CofferPayoutInterval = 25000;            // In Blocks. Each blocks generated in 20-30 seconds.

        public static readonly BigInteger bigCity = 1, mediumCity = 2, smallCity = 3;

        /**
         * Entry Point of Smartcontract on Neo Blockchain + C#
         * 
         * @Param (BigInteger) - Function Name that should be called
         * @args  (Object[])  - Arguments of Function that will be called. Function name is given by @param.
         */
        public static byte[] Main(string param, object[] args)
        {
            byte[] arg1 = (byte[])args[1];
            byte[] arg2 = (byte[])args[2];
            byte[] arg3 = (byte[])args[3];
            string arg4 = (string)args[4];
            string key = "keyValue";

            Runtime.Notify("Init!!!");

            string sign = arg1.AsString();// arg1.AsString();
            sign = sign + arg2.AsString();// Neo.SmartContract.Framework.Helper.Concat((byte[])args[0], (byte[])args[1]);
            sign = sign + arg3.AsString();// sign.Concat(arg3);
            sign = sign + arg4;// Concat(arg4);
            sign = sign + key;

            Runtime.Notify("String is ", sign);

            byte[] bytes = sign.AsByteArray();

            byte[] hashed = Hash256(bytes);

            byte[] signature = (byte[])args[0];
            if ( signature.Equals(hashed) )
            {
                Runtime.Notify("Signature is verified");
            } else
            {
                Runtime.Notify("Signature is not verified");
            }
            
            Runtime.Notify("Sign that was generated", hashed, "For", sign, "inbytes", bytes, "Sign that comes", (byte[])args[0], "For", (string)args[1], "in bytes", (byte[])args[2]);
            
            //return new BigInteger(1).AsByteArray();

            if (param.Equals("cofferPayout"))
            {
                return CofferPayout(args);
            }
            else if (param.Equals("putCityData"))
            {
                return Put.Cities(args);
            }
            else if (param.Equals("auctionBegin"))
            {
                Runtime.Log("Calling: Auction Begin");

                MarketItemData marketItem = new MarketItemData();
                marketItem.AuctionDuration = (BigInteger)args[1];
                marketItem.Price = (BigInteger)args[2];
                marketItem.City = (byte)args[3];
                marketItem.AuctionStartedTime = Blockchain.GetBlock(Blockchain.GetHeight()).Timestamp;
                marketItem.Seller = (byte[])args[4];

                Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
                marketItem.TX = TX.Hash;

                Runtime.Notify("Price is ", marketItem.Price, "Incoming price", (BigInteger)args[2]);

                return Auction.Begin((BigInteger)args[0], marketItem);
            }
            
            else if (param.Equals("putItem"))
            {
                Runtime.Log("Put Item on Storage");

                // Invoker has permission to execute this function?
                if (!Runtime.CheckWitness(GameOwner))
                {
                    Runtime.Log("Permission denied! Only game admin can add new items. Atleast for now!");
                    return new BigInteger(0).AsByteArray();
                }

                if (args.Length != 7)
                {
                    Runtime.Log("Invalid parameters."); // This function has 7 parameters
                    return new BigInteger(0).AsByteArray();
                }

                // Item given type: for hero creation or drop for stronghold
                if ((BigInteger)args[0] != STRONGHOLD_REWARD && (BigInteger)args[0] != HERO_CREATION_GIVEN)
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

                Put.Item((BigInteger)args[1], (byte)args[0], item);
            }
            
            else if (param.Equals("putHero"))
            {
                if (args.Length != 14)
                {
                    Runtime.Log("Invalid amount parameters");
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
                hero.TX = ((Transaction)ExecutionEngine.ScriptContainer).Hash;

                return Put.Hero((BigInteger)args[0], hero, (byte[])args[8], (BigInteger)args[9], (BigInteger)args[10], (BigInteger)args[11], (BigInteger)args[12], (BigInteger)args[13]);
            }
            else if (param.Equals("auctionEnd"))
            {
                // Check Does item exist
                Runtime.Log("Calling Auction End");

                //return new BigInteger(0).AsByteArray();
                return Auction.End((BigInteger)args[0], (byte[])args[1]);
            }
            else if (param.Equals("auctionCancel"))
            {
                return Auction.Cancel((BigInteger)args[0]);
            }
            /**
             * Function records item drop
             * 
             * Function drops item in every 120 blocks. Usually called by Server Side of Blocklords.
             * 
             * Has 0 argument
             */
            else if (param.Equals("dropItems"))
            {
                return DropItems();
            }
            /**
             * Function records Battle result: Attack on City. 
             * 
             * Attacker of City invokes this function.
             * 
             * Has 20 arguments
             * @Battle ID (BigInteger)                  - Unique ID of Battle
             * @Battle Result (BigInteger)              - 0 means Attacker Won, 1 means Attacker Lose
             * @Attacker (BigInteger)                   - ID of Hero that initialized battle
             * @Attacker Owner (byte[])                 - Wallet Address of Hero's Owner
             * @Attacker Troops (BigInteger)                   - Amount of troops that were involved in the battle
             * @Attacker Remained Troops (BigInteger)          - Amount of troops that remained after battle
             * @Attacker Equipped Item #1 (BigInteger)         - Item that was equipped by Hero during Battle
             * @Attacker Equipped Item #2 (BigInteger)
             * @Attacker Equipped Item #3 (BigInteger)
             * @Attacker Equipped Item #4 (BigInteger)
             * @Attacker Equipped Item #5 (BigInteger)
             * 
             * @Defender (BigInteger)                           - City or Stronghold owning Hero's ID or NPC id.
             * @Defender Owner (byte[])                         - If Battle Initiator attacked City or Stronghold, then the wallet address of City or Stronghold owner
             * @Defender Troops (BigInteger)                    - Amount of troops that were involved in the battle
             * @Defender Remained Troops (BigInteger)           - Amount of troops that remained after battle
             * @Defender Equipped Item #1 (BigInteger)          - Item that was equipped by Hero during Battle
             * @Defender Equipped Item #2 (BigInteger)
             * @Defender Equipped Item #3 (BigInteger)
             * @Defender Equipped Item #4 (BigInteger)
             * @Defender Equipped Item #5 (BigInteger)
             * 
             * @Defender's Object (BigInteger)                  - Is It NPC, CITY or STRONGHOLD that was attacked by Battle Initiator
             */
            else if (param.Equals("LogCityAttack"))
            {
                Runtime.Log("Initialize city attack");
                return Log.CityAttack(args);
            }
            else if (param.Equals("LogStrongholdAttack"))
            {
                Runtime.Log("Initialize stronghold attack");
                return Log.StrongholdAttack(args);
            }
            else if (param.Equals("LogBanditCampAttack"))
            {
                Runtime.Log("Initialize bandir camp attack");
                return Log.BanditCampAttack(args);
            }
            else if (param.Equals("LogStrongholdLeave"))
            {
                return Log.StrongholdLeave(args);
            }
            // Add Auction Cancel

            //Runtime.Notify("Incorrect Parameter");
            return new BigInteger(1).AsByteArray();
        }

        
        
        //------------------------------------------------------------------------------------
        //
        // functions for:
        // ITEM DROP AS A REWARD
        //
        //------------------------------------------------------------------------------------

        public static byte[] DropItems()
        {
            // Between each Item Drop as a reward should be generated atleast 120 Blocks
            DropData dropData = (DropData)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, LATEST_REWARDED_ITEM_KEY));

            BigInteger currentBlock = Blockchain.GetHeight();

            if (dropData != null && currentBlock < dropData.Block + DropInterval)
            {
                Runtime.Notify("Too early to drop Items! Last Block", dropData.Block, "Current Block", currentBlock);
                return new BigInteger(0).AsByteArray();
            }

            BigInteger totalStrongholdBlocks = 0;
            BigInteger[] blockStarts = new BigInteger[10];
            BigInteger[] blockEnds = new BigInteger[10];

            string key = "";
            Stronghold stronghold;

            // There are 10 Strongholds on the map
            for (int i = 0; i < 10; i++)            // i + 1 = Stronghold ID
            {
                key = STRONGHOLD_PREFIX + (i + 1).Serialize();

                byte[] strongholdBytes = Storage.Get(Storage.CurrentContext, key);

                if (strongholdBytes.Length == 0)
                {
                    blockStarts[i] = blockEnds[i] = 0;      // Stronghold has no Owner. Skipping it
                }
                else
                {
                    stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(strongholdBytes);

                    if (i == 0)
                        blockStarts[i] = 0;
                    else
                        blockStarts[i] = blockEnds[i - 1];

                    BigInteger blocks = Blockchain.GetHeight() - stronghold.CreatedBlock;

                    blockEnds[i] = blockStarts[i] + blocks;

                    totalStrongholdBlocks += blocks;
                }


            }

            if (totalStrongholdBlocks == 0)
            {
                Runtime.Notify("There are no Stronghold owners");
                return new BigInteger(0).AsByteArray();
            }
            else
            {
                BigInteger randomBlock = GetRandomNumber((ulong)totalStrongholdBlocks);
                Runtime.Notify("Returned Random Stronghold Block", randomBlock, "Define Stronghold ID");  // To Debug

                // Find Stronghold based on block
                for (var i = 0; i < 10; i++)
                {
                    if (randomBlock >= blockStarts[i] && randomBlock <= blockEnds[i])    //TODOOOOOOOOOOOOOOOOOOOOOOOOOOO if random block will be equal to min or max, this logic command will return FALSE?
                    {
                        // Is Stronghold has an owner?
                        key = STRONGHOLD_PREFIX + (i + 1).Serialize();
                        break;
                    }
                }



                byte[] strongholdBytes = Storage.Get(Storage.CurrentContext, key);
                stronghold = (Stronghold)Neo.SmartContract.Framework.Helper.Deserialize(strongholdBytes);

                //if (stronghold.Hero <= 0)
                //{
                //    Runtime.Notify("Stronghold is owned by NPC");
                //    return new BigInteger(0).AsByteArray();
                //}

                // The Stronghold was randomly selected. Now, give to owner of Stronghold the item
                var nextRewardItem = Storage.Get(Storage.CurrentContext, NEXT_REWARD_ITEM_KEY);
                if (nextRewardItem.Length == 0)
                {
                    Runtime.Notify("Item to reward for stronghold owning doesn't exist");
                    return new BigInteger(0).AsByteArray();
                }



                string itemKey = ITEM_PREFIX + nextRewardItem;
                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, itemKey));

                string heroKey = HERO_PREFIX + stronghold;
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, heroKey));


                // Change owner of Item.
                item.OWNER = hero.OWNER;
                byte[] itemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

                // Save Item Owner Changing
                Storage.Put(Storage.CurrentContext, itemKey, itemBytes);

                // Delete Stronghold owner. (It means "kicking out from Stronghold"). According to rule, if stronghold owner got item, he should be kicked out from stronghold
                Storage.Delete(Storage.CurrentContext, key);

                // Since, latest item on batch is given, delete it and set previous added item on batch as a next item to reward.

                key = STRONGHOLD_REWARD_ITEM_KEY_PREFIX + nextRewardItem;
                BigInteger previousItem = new BigInteger(Storage.Get(Storage.CurrentContext, key));
                Storage.Delete(Storage.CurrentContext, key);
                Storage.Put(Storage.CurrentContext, NEXT_REWARD_ITEM_KEY, previousItem); // Set Next Reward Item

                // Set Stronghold update time.
                DropData dropped = new DropData();
                dropped.Block = Blockchain.GetHeight();
                dropped.HeroId = stronghold.Hero;
                dropped.ItemId = nextRewardItem.AsBigInteger();
                dropped.StrongholdId = stronghold.ID;

                byte[] bytes = Neo.SmartContract.Framework.Helper.Serialize(dropped);

                Storage.Put(Storage.CurrentContext, LATEST_REWARDED_ITEM_KEY, bytes);
            }

            Runtime.Notify("Item was Dropped successfully");
            return new BigInteger(1).AsByteArray();
        }


        //------------------------------------------------------------------------------------
        //
        // City Coffers Payout
        //
        //------------------------------------------------------------------------------------
        public static byte[] CofferPayout(object[] args)
        {
            // Check Witness
            if (!Runtime.CheckWitness(GameOwner))
            {
                Runtime.Log("Permission denied! Only game admin can use this function!");
                return new BigInteger(0).AsByteArray();
            }

            // TODO check Coffer payment interval

            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);

            // City Coffer Cut.
            for (var i = 0; i < args.Length; i++)
            {
                string key = CITY_PREFIX + ((BigInteger)args[i]).AsByteArray();
                byte[] cityBytes = Storage.Get(Storage.CurrentContext, key);



                if (cityBytes.Length == 0)
                {
                    Runtime.Notify("City doesn't exist!");
                    return new BigInteger(0).AsByteArray();
                }

                City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);

                if (city.Hero == 0)
                {
                    continue;
                }

                // Coffer is 30%

                string heroKey = HERO_PREFIX + city.Hero.AsByteArray();
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, key));

                BigInteger percent = city.Coffer / 100;
                BigInteger requiredValue = percent * 30;

                foreach (var payOut in outputs)
                {
                    BigInteger incomingValue = payOut.Value;
                    if (incomingValue == requiredValue && hero.OWNER == payOut.ScriptHash)
                    {
                        city.Coffer = city.Coffer - requiredValue;

                        cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);

                        Storage.Put(Storage.CurrentContext, key, cityBytes);

                        continue;
                    }
                }

                Runtime.Notify("Hero doesn't receive his coffer prize!!!");
                return new BigInteger(0).AsByteArray();
            }

            return new BigInteger(1).AsByteArray();
        }

        

        //------------------------------------------------------------------------------------
        //
        // Helpers used in Smartcontract
        //
        //------------------------------------------------------------------------------------
        public static BigInteger GetRandomNumber(ulong max = 10)
        {
            Header header = Blockchain.GetHeader(Blockchain.GetHeight());
            ulong randomNumber = header.ConsensusData;
            int percentage = (int)(randomNumber % max);

            return new BigInteger(percentage);
        }

        private static bool IsAuctionTransactionFeeIncluded(BigInteger duration)
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
    }
}
