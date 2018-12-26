using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System.Numerics;

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

        public static readonly BigInteger DropInterval = 120; // Item will be dropped in every 120 blocks. On Neo Blockchain, each block is generated within 20-30 seconds.

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

            if (param.Equals("cofferPayout"))
            {
                return Periodical.CofferPayout(args);
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
            else if (param.Equals("dropItems"))
            {
                return Periodical.DropItems();
            }
            
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


    }
}
