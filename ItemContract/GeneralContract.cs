using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

namespace LordsContract
{
    /// <summary>
    /// GeneralContract inherited from Neo.SmartContract!
    /// 
    /// This class is holding <paramref name="Main" /> method, the entry point of Smartcontract.
    /// As well as definitions of helper Methods and Properties that are accessable throughout all project parts.
    /// </summary>
    public class GeneralContract : SmartContract
    {
        /// <summary>
        /// Storage Map Prefix to generate keys of Market Item on Storage.
        /// </summary>
        public static readonly string MARKET_MAP = "\x01\x00";
        /// <summary>
        /// Storage Map Prefix to generate keys of Items on Storage
        /// </summary>
        public static readonly string ITEM_MAP = "\x02\x00";
        /// <summary>
        /// Storage Map Prefix to generate keys of Strongholds on Storage
        /// </summary>
        public static readonly string STRONGHOLD_MAP = "\x03\x00";
        /// <summary>
        /// Storage Map Prefix to generate keys of Stronghold Reward Items batch on storage
        /// </summary>
        public static readonly string STRONGHOLD_REWARD_BATCH_MAP = "\x04\x00";
        /// <summary>
        /// Storage Map Prefix to generate keys of Cities on Storage
        /// </summary>
        public static readonly string CITY_MAP = "\x07\x00";
        /// <summary>
        /// Storage Map Prefix to generate keys of Heroes on Storage.
        /// </summary>
        public static readonly string HERO_MAP = "\x08\x00";
        /// <summary>
        /// Storage Key of Hero Creation Fee
        /// </summary>
        public static readonly string FEE_HERO_CREATION = "\x16";
        /// <summary>
        /// Storage Key of Refering Fee
        /// </summary>
        public static readonly string FEE_REFERAL = "\x17";
        /// <summary>
        /// Storage Key of fee for 8 hours of item appearance on market
        /// </summary>
        public static readonly string FEE_8_HOURS = "\x18";
        /// <summary>
        /// Storage Key of fee for 12 hours of item appearance on market
        /// </summary>
        public static readonly string FEE_12_HOURS = "\x19";
        /// <summary>
        /// Storage Key of fee for 24 hours of item appearance on market
        /// </summary>
        public static readonly string FEE_24_HOURS = "\x20";
        /// <summary>
        /// Storage Key of fee for city attack
        /// </summary>
        public static readonly string FEE_PVC = "\x21";
        /// <summary>
        /// Storage Key of fee for bandit camp attack
        /// </summary>
        public static readonly string FEE_PVE = "\x22";
        /// <summary>
        /// Storage Key of fee for stronghold attack
        /// </summary>
        public static readonly string FEE_PVP = "\x23";
        /// <summary>
        /// Storage Key of GAS in percents that buyer should attach to buy item.
        /// As a base sum for calculation of GAS in percents is used the market item price.
        /// </summary>
        public static readonly string PERCENTS_PURCHACE = "\x24";
        /// <summary>
        /// Storage Key of GAS in percents that lord of a city should get from buyer.
        /// As a base sum for calculation of GAS in percents that lord of a city should get is used market item price
        /// </summary>
        public static readonly string PERCENTS_LORD = "\x25";
        /// <summary>
        /// Storage Key of GAS in percents that seller of item will get from buyer.
        /// As a base sum for calculation of GAS is percents that seller will get is used the market item price.
        /// </summary>
        public static readonly string PERCENTS_SELLER_COFFER = "\x26";
        /// <summary>
        /// Storage Key of GAS attachments in percents of City Attacks,
        /// that will be transfered to city coffer  
        /// </summary>
        public static readonly string PERCENTS_PVC_COFFER = "\x27";
        /// <summary>
        /// Storage Key of GAS in percents that will be sent transferred to player.
        /// </summary>
        public static readonly string PERCENTS_COFFER_PAY = "\x28";
        /// <summary>
        /// Storage Key of coffer drop interval in blocks.
        /// </summary>
        public static readonly string INTERVAL_COFFER = "\x29";
        /// <summary>
        /// Storage Key of stronghold reward interval in blocks
        /// </summary>
        public static readonly string INTERVAL_STRONGHOLD_REWARD = "\x30";
        /// <summary>
        /// Tracks strongholds amount on Contract
        /// </summary>
        public static readonly string AMOUNT_STRONGHOLDS = "\x31";
        /// <summary>
        /// Tracks cities amount on Contract
        /// </summary>
        public static readonly string AMOUNT_CITIES = "\x32";


        /// <summary>
        /// Battle type
        /// </summary>
        public static readonly BigInteger PVC = 0, PVP = 1, PVE = 2;

        /// <summary>
        /// Item batch type
        /// </summary>
        public static readonly byte HERO_CREATION_BATCH = 0, STRONGHOLD_REWARD_BATCH = 1, NO_BATCH = 2;

        /**
         * 1 GAS === 100_000_000
         * 0.1GAS == 10_000_000
         * 
         * Neo Blockchain's Virtual Machine, where Smartcontracts are executed doesn't support Float numbers.
         * So all incoming Float numbers are converted and used in multiplication of 100_000_000.
         * 
         * Basically 0.1 means 10000000 (10_000_000) during Execution of Smartcontract.
         */
        //public static readonly decimal auction8HoursFee = 10_000_000,              // 0.1 GAS
        //                               auction12HoursFee = 20_000_000,          // 0.2 GAS
        //                               auction24HoursFee = 30_000_000,          // 0.3 GAS
        //                               heroCreationFee = 500_000_000,           // 5.0 GAS
        //                               cityAttackFee = 200_000_000,             // 2.0 GAS
        //    strongholdAttackFee = 100_000_000,                                       // 1.0 GAS
        //    banditCampAttackFee = 50_000_000,                                       // 0.5 GAS
        //    bigCityCoffer = 100_000_000,                                      // 1.0 GAS
        //    mediumCityCoffer = 88_000_000,                                       // 0.8 GAS
        //    smallCityCoffer = 50_000_000                                       // 0.5 GAS
        //    ;

        public static readonly BigInteger duration8Hours = 28800, duration12Hours = 43200, duration24Hours = 86400;                 // 86_400 Seconds are 24 hours

        // The Smartcontract Owner's Wallet Address. Used to receive some Gas as a transaction fee.

        /// <summary>
        /// Game Owner's script hash
        /// </summary>
        public static readonly byte[] GameOwner = "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y".ToScriptHash();//"AML8hyTV4vXuomovxdcAH9pRC9ny618YmA".ToScriptHash();
        public static readonly BigInteger CofferPayoutInterval = 25000;            // In Blocks. Each blocks generated in 20-30 seconds.

        /// <summary>
        /// City type
        /// </summary>
        public static readonly BigInteger CITY_TYPE_BIG = 1, CITY_TYPE_MID = 2, CITY_TYPE_SMALL = 3;
                
        /// <summary>
        /// Entry point of smartcontract
        /// </summary>
        /// <param name="param">Method name</param>
        /// <param name="args">method arguments array</param>
        /// <returns>1 if success, 0 if failed</returns>
        public static byte[] Main(string param, object[] args)
        {
            if (param.Equals("setSetting"))
            {
                Runtime.Log("Set Settings");
                return Settings.Set((string)args[0], (BigInteger)args[1]);
            }
            else if (param.Equals("cofferPayout"))
            {
                return Periodical.CofferPayout();
            }
            else if (param.Equals("dropItems"))
            {
                return Periodical.SimpleDropItems((BigInteger)args[0]);
            }
            else if (param.Equals("putCity"))
            {
                return Put.City((BigInteger)args[0], (BigInteger)args[1], (BigInteger)args[2]);
            }
            else if (param.Equals("putStronghold"))
            {
                //string key;
                //Stronghold stronghold = new Stronghold();
                //byte[] bytes;

                //stronghold.Hero = (BigInteger)args[1];
                //stronghold.ID = (BigInteger)args[0];
                //stronghold.CreatedBlock = Blockchain.GetHeight();

                //key = GeneralContract.STRONGHOLD_MAP + stronghold.ID.AsByteArray();
                //bytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);

                //Storage.Put(Storage.CurrentContext, key, bytes);

                //Runtime.Log("Stronghold data has been inserted");

                //return new BigInteger(0).AsByteArray();
                return Put.Stronghold((BigInteger)args[0]);
            }
            else if (param.Equals("putItem"))
            {
                Runtime.Log("Put Item on Storage");
                if (args.Length != 6)
                {
                    Runtime.Log("Invalid parameters."); // This function has 7 parameters
                    return new BigInteger(0).AsByteArray();
                }

                // Item given type: for hero creation or drop for stronghold
                if ((BigInteger)args[0] != STRONGHOLD_REWARD_BATCH && (BigInteger)args[0] != HERO_CREATION_BATCH)
                {
                    Runtime.Log("Given method of Item is invalid");
                    return new BigInteger(0).AsByteArray();
                }

                // Item Parameters
                Item item = new Item();

                item.STAT_TYPE = (BigInteger)args[2];       // 1 length
                item.QUALITY = (BigInteger)args[3];         // 1 length
                item.GENERATION = (BigInteger)args[4];      // ???

                item.STAT_VALUE = (BigInteger)args[5];      // ???
                item.LEVEL = 0;           // ???
                //item.OWNER = new byte[] { };                // 20
                item.XP = 0;                                // ???
                item.HERO = 0;
                //item.INITIAL = (BigInteger)args[7];         // 1
                //item.OWNER = ExecutionEngine.CallingScriptHash;
                item.BATCH = (byte)args[0];

                Put.Item((BigInteger)args[1], item);
            }
            else if (param.Equals("putHero"))
            {
                //if (args.Length != 14)
                //{
                //Runtime.Log("Invalid amount parameters");
                //return new BigInteger(0).AsByteArray();
                //}
                // If Referer ID is 0, do not check it.
                // If referer id is not 0 but hero doesn't exist on Blockchain, throw an Error
                // Check if there are referer fee existing

                if (Runtime.CheckWitness(GameOwner))
                {
                    Runtime.Log("GAME_OWNER_DO_NOT_ALLOWED_TO_PLAY_GAME");
                    throw new System.Exception("GAME_OWNER_DO_NOT_ALLOWED_TO_PLAY_GAME");
                }
                else
                {
                    Runtime.Log("Smartcontract is called from average player!");
                }

                byte[] scriptHash = (byte[])args[0];
                if (!Runtime.CheckWitness(scriptHash))
                {
                    Runtime.Log("MISSED_CALLER_REVERSED_SCRIPT_HASH");
                    throw new System.Exception("MISSED_CALLER_REVERSED_SCRIPT_HASH");
                }

                BigInteger heroId = (BigInteger)args[1];
                BigInteger refererHeroId = (BigInteger)args[2];
                byte[] refererScriptHash = (byte[])args[3];

                if (heroId <= 0)
                {
                    Runtime.Log("HERO_ID_MUST_BE_GREATER_THAN_0");
                    throw new System.Exception("HERO_ID_MUST_BE_GREATER_THAN_0");
                }

                string heroKey = HERO_MAP + heroId.AsByteArray();
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                if (heroBytes.Length > 0)
                {
                    Runtime.Log("HERO_ON_BLOCKCHAIN_ALREADY");
                    throw new System.Exception("HERO_ON_BLOCKCHAIN_ALREADY");
                }

                if (refererHeroId > 0)
                {
                    string refererHeroKey = HERO_MAP + refererHeroId.AsByteArray();
                    byte[] refererHeroBytes = Storage.Get(Storage.CurrentContext, refererHeroKey);
                    if (refererHeroBytes.Length <= 0)
                    {
                        Runtime.Log("REFERER_HERO_MUST_BE_ON_BLOCKCHAIN");
                        throw new System.Exception("REFERER_HERO_MUST_BE_ON_BLOCKCHAIN");
                    }

                    Hero refererHero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(refererHeroBytes);

                    if (Runtime.CheckWitness(refererHero.OWNER))
                    {
                        Runtime.Log("CAN_NOT_REFER_BY_YOURSELF");
                        throw new System.Exception("CAN_NOT_REFER_BY_YOURSELF");
                    }

                    if (refererHero.OWNER.AsBigInteger() != refererScriptHash.AsBigInteger())
                    {
                        Runtime.Log("INVALID_REFERER_SCRIPT_HASH_GIVEN");
                        throw new System.Exception("INVALID_REFERER_SCRIPT_HASH_GIVEN");
                    }

                    byte[] feeBytes = Storage.Get(Storage.CurrentContext, FEE_HERO_CREATION);
                    BigInteger fee = feeBytes.AsBigInteger();

                    byte[] refererBytes = Storage.Get(Storage.CurrentContext, FEE_REFERAL);
                    BigInteger refererFee = refererBytes.AsBigInteger();

                    // Game owner's fee is less, if we have a referer
                    fee = BigInteger.Subtract(fee, refererFee);

                    if (!AttachmentExist(refererFee, refererHero.OWNER))
                    {
                        Runtime.Log("REFERER_FEE_MUST_BE_INCLUDED");
                        throw new System.Exception();
                    }

                    if (!AttachmentExist(fee, GameOwner))
                    {
                        Runtime.Log("HERO_CREATION_FEE_MUST_BE_INCLUDED");
                        throw new System.Exception();
                    }
                }
                else
                {
                    byte[] feeBytes = Storage.Get(Storage.CurrentContext, FEE_HERO_CREATION);
                    BigInteger fee = feeBytes.AsBigInteger();
                    if (!AttachmentExist(fee, GameOwner))
                    {
                        Runtime.Log("HERO_CREATION_FEE_MUST_BE_INCLUDED");
                        throw new System.Exception();
                    }

                }

                BigInteger[] stats = (BigInteger[])args[4]; 
                if (stats.Length != 5)
                {
                    Runtime.Log("STATS_NUMBER_SHOULD_BE_5");
                    throw new Exception();
                }

                Hero hero3 = new Hero();
                hero3.OWNER = scriptHash;
                hero3.INTELLIGENCE = stats[2];
                hero3.SPEED = stats[3];
                hero3.STRENGTH = stats[4];
                hero3.LEADERSHIP = stats[1];
                hero3.DEFENSE = stats[0];

                return Put.Hero(heroId, hero3);

                Runtime.Log("Finish");
                return new BigInteger(0).AsByteArray();
                // Items parameters must be included
                // Items on blockchain must be on
                // Items on blockchain should be on hero batch

                Runtime.Log("Hero putting initialized");
                Runtime.Notify(refererScriptHash);
                Hero hero = new Hero();
                hero.OWNER = ExecutionEngine.CallingScriptHash;
                hero.INTELLIGENCE = (BigInteger)args[3];
                hero.SPEED = (BigInteger)args[4];
                hero.STRENGTH = (BigInteger)args[5];
                hero.LEADERSHIP = (BigInteger)args[6];
                hero.DEFENSE = (BigInteger)args[7];

                // Change Item Owners
                Helper.ChangeItemOwner((BigInteger)args[7], hero.ID);
                Helper.ChangeItemOwner((BigInteger)args[8], hero.ID);
                Helper.ChangeItemOwner((BigInteger)args[9], hero.ID);
                Helper.ChangeItemOwner((BigInteger)args[10], hero.ID);
                Helper.ChangeItemOwner((BigInteger)args[11], hero.ID);

                return Put.Hero((BigInteger)args[0], hero);
            }
            else if (param.Equals("marketAddItem"))
            {
                // 1: Item ID, 2: Auction Duration, 3: Price, 4: City ID, 5: Seller ID
                if (args.Length != 5)
                {
                    Runtime.Log("Invalid parameters."); // This command has 5 parameters
                    return new BigInteger(0).AsByteArray();
                }

                MarketItemData marketItem = new MarketItemData();
                marketItem.Duration = (BigInteger)args[1];
                marketItem.Price = (BigInteger)args[2];
                marketItem.City = (BigInteger)args[3];
                marketItem.CreatedTime = Blockchain.GetBlock(Blockchain.GetHeight()).Timestamp;
                marketItem.Seller = ExecutionEngine.CallingScriptHash;

                Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
                //marketItem.TX = TX.Hash;

                Runtime.Notify("Price is ", marketItem.Price, "Incoming price", (BigInteger)args[2]);

                return Market.AddItem((BigInteger)args[0], marketItem);
            }
            else if (param.Equals("marketBuyItem"))
            {
                // Check Does item exist
                Runtime.Log("Calling Auction End");

                //return new BigInteger(0).AsByteArray();
                return Market.BuyItem((BigInteger)args[0]);
            }
            else if (param.Equals("marketDeleteItem"))
            {
                return Market.DeleteItem((BigInteger)args[0]);
            }
            else if (param.Equals("logBattle"))
            {
                return Log.Battle(args);
            }

            //Runtime.Notify("Incorrect Parameter");
            return new BigInteger(1).AsByteArray();
        }

        /// <summary>
        /// Retrieve pseudo random number
        /// </summary>
        /// <param name="max">Max range</param>
        /// <returns>generated number</returns>
        public static BigInteger GetRandomNumber(ulong max = 10)
        {
            Header header = Blockchain.GetHeader(Blockchain.GetHeight());
            ulong randomNumber = header.ConsensusData;
            ulong percentage = (randomNumber % max);

            BigInteger bigRandom = percentage;

            return bigRandom;
        }

        /// <summary>
        /// Checks whether given sum of GAS is attached or not?
        /// </summary>
        /// <param name="value">sum of GAS</param>
        /// <returns>true if attached, false if not</returns>
        public static bool IsTransactionOutputExist(BigInteger value)
        {
            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            foreach (var output in outputs)
            {
                // Game Developers got their fee?
                if (output.ScriptHash.AsBigInteger() == GeneralContract.GameOwner.AsBigInteger())
                {
                    Runtime.Notify("Game Owner received ", output.Value);
                    if (output.Value == value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool AttachmentExist(BigInteger value, byte[] receivingScriptHash)
        {
            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();
            Runtime.Notify("Outputs are", outputs.Length);
            foreach (var output in outputs)
            {
                // Game Developers got their fee?
                if (output.ScriptHash.AsBigInteger() == receivingScriptHash.AsBigInteger())
                {
                    if (output.Value == value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
