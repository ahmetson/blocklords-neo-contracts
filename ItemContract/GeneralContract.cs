using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.ComponentModel;
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
        /// Storage Key of GAS in percents that city coffer will get for every added market item.
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

        [DisplayName("heroCreation")]
        public static event Action<BigInteger, byte[], BigInteger[], BigInteger[]> heroCreation;

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
                item.BATCH = (BigInteger)args[0];

                Put.Item((BigInteger)args[1], item, false);
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

                BigInteger[] equipments = (BigInteger[])args[5];
                if (equipments.Length != 5)
                {
                    Runtime.Log("EQUIPMENTS_NUMBER_SHOULD_BE_5");
                    throw new Exception();
                }

                // Change Item Owners
                Helper.ChangeItemOwner(equipments[0], heroId);
                Helper.ChangeItemOwner(equipments[1], heroId);
                Helper.ChangeItemOwner(equipments[2], heroId);
                Helper.ChangeItemOwner(equipments[3], heroId);
                Helper.ChangeItemOwner(equipments[4], heroId);

                byte[] result = Put.Hero(heroId, hero3);

                heroCreation(heroId, scriptHash, stats, equipments);

                /// Weird, command below fails, with Referer code.
                /// Add fee
                //Storage.Put(Storage.CurrentContext, scriptHash, heroId);

                return result;
            }
            else if (param.Equals("marketAddItem"))
            {
                // 1: Hero Id, 2: Item Id, 3: Price, 4: Duration in seconds, 5: City ID
                if (args.Length != 5)
                {
                    Runtime.Log("Invalid parameters."); // This command has 5 parameters
                    return new BigInteger(0).AsByteArray();
                }

                BigInteger itemId = (BigInteger)args[0];
                BigInteger price = (BigInteger)args[1];
                BigInteger duration = (BigInteger)args[2];
                BigInteger cityId = (BigInteger)args[3];

                if (Runtime.CheckWitness(GameOwner))
                {
                    Runtime.Log("GAME_OWNER_CAN_NOT_PLAY_GAME");
                    throw new Exception();
                }

                byte[] durationFeeBytes = new BigInteger(0).AsByteArray();
                if (duration == duration8Hours)
                {
                    durationFeeBytes = Storage.Get(Storage.CurrentContext, FEE_8_HOURS);
                }
                else if (duration == duration12Hours)
                {
                    durationFeeBytes = Storage.Get(Storage.CurrentContext, FEE_12_HOURS);
                }
                else if (duration == duration24Hours)
                {
                    durationFeeBytes = Storage.Get(Storage.CurrentContext, FEE_24_HOURS);
                }
                else { 
                    Runtime.Log("DURATION_MUST_BE_VALID_AND_IN_SECONDS");
                    throw new Exception();
                }

                string cityKey = CITY_MAP + cityId.AsByteArray();
                byte[] cityBytes = Storage.Get(Storage.CurrentContext, cityKey);
                if (cityBytes.Length <= 0)
                {
                    Runtime.Log("CITY_MUST_BE_ON_BLOCKCHAIN");
                    throw new Exception();
                }

                City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);
                
                if (city.ItemsOnMarket >= city.ItemsCap)
                {
                    Runtime.Log("CITY_MARKET_MUST_BE_NON_FULL");
                    throw new Exception();
                }

                byte[] itemIdBytes = itemId.AsByteArray();

                string itemKey = ITEM_MAP + itemIdBytes;
                byte[] itemBytes = Storage.Get(Storage.CurrentContext, itemKey);

                if (itemBytes.Length <= 0)
                {
                    Runtime.Log("ITEM_MUST_BE_ON_BLOCKCHAIN");
                    throw new Exception();
                }

                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes);
                if (item.HERO <= 0)
                {
                    Runtime.Log("ITEM_HOLDING_HERO_MUST_ADD_ITEM_ONTO_MARKET");
                    throw new Exception();
                }

                string heroKey = HERO_MAP + item.HERO.AsByteArray();
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                if (heroBytes.Length <= 0)
                {
                    Runtime.Log("ITEM_SELLER_MUST_BE_ON_BLOCKCHAIN");
                    throw new Exception();
                }

                // Seller
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
                if (!Runtime.CheckWitness(hero.OWNER))
                {
                    Runtime.Log("ITEM_OWNER_MUST_SELL_ITEM");
                    throw new Exception();
                }

                string marketItemKey = MARKET_MAP + itemId.AsByteArray();
                byte[] marketItemBytes = Storage.Get(Storage.CurrentContext, marketItemKey);
                if (marketItemBytes.Length > 0)
                {
                    MarketItemData oldMarketItem = (MarketItemData)Neo.SmartContract.Framework.Helper.Deserialize(marketItemBytes);
                    if (oldMarketItem.Duration + oldMarketItem.CreatedTime <= Blockchain.GetBlock(Blockchain.GetHeight()).Timestamp)
                    {
                        Runtime.Log("ITEM_MUST_BE_NOT_ON_MARKET");
                        throw new Exception();
                    }
                    else
                    {
                        Runtime.Notify("Market item duration is expired, duration and created time", oldMarketItem.Duration, oldMarketItem.CreatedTime);
                    }
                }

                BigInteger durationFee = durationFeeBytes.AsBigInteger();
                if (!AttachmentExist(durationFee, GameOwner))
                {
                    Runtime.Log("ATTACHMENT_FEE_MUST_BE_INCLUDED");
                    throw new Exception();
                }

                MarketItemData marketItem = new MarketItemData();
                marketItem.Duration = duration;
                marketItem.Price = price;
                marketItem.City = cityId;
                marketItem.CreatedTime = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;
                marketItem.Seller = hero.OWNER;

                Runtime.Notify("Price is ", marketItem.Price, "Incoming price", price);

                marketItemBytes = Neo.SmartContract.Framework.Helper.Serialize(item);

                // Save on Storage!!!
                Storage.Put(Storage.CurrentContext, marketItemKey, marketItemBytes);

                // Increase coffer
                byte[] purchaseCofferPercentsBytes = Storage.Get(Storage.CurrentContext, PERCENTS_SELLER_COFFER);
                BigInteger purchaseCofferPercents = purchaseCofferPercentsBytes.AsBigInteger();
                BigInteger sellFeePercents = BigInteger.Divide(durationFee, 100);
                BigInteger purchaseCoffer = BigInteger.Multiply(purchaseCofferPercents, sellFeePercents);
                city.Coffer = BigInteger.Add(city.Coffer, purchaseCoffer);
                city.ItemsOnMarket = BigInteger.Add(city.ItemsOnMarket, 1);

                //// Update City
                cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
                Storage.Put(Storage.CurrentContext, cityKey, cityBytes);


                Runtime.Log("Item added onto market");
                return new BigInteger(0).AsByteArray();
            }
            else if (param.Equals("marketBuyItem"))
            {
                BigInteger heroId = (BigInteger)args[0];
                BigInteger itemId = (BigInteger)args[1];

                if (Runtime.CheckWitness(GameOwner))
                {
                    Runtime.Log("GAME_OWNER_CAN_NOT_PLAY_GAME");
                    throw new Exception();
                }

                byte[] itemIdBytes = itemId.AsByteArray();

                string itemKey = ITEM_MAP + itemIdBytes;
                byte[] itemBytes = Storage.Get(Storage.CurrentContext, itemKey);

                if (itemBytes.Length <= 0)
                {
                    Runtime.Log("ITEM_MUST_BE_ON_BLOCKCHAIN");
                    throw new Exception();
                }

                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes);
                if (item.HERO <= 0)
                {
                    Runtime.Log("ITEM_HOLDING_HERO_MUST_ADD_ITEM_ONTO_MARKET");
                    throw new Exception();
                }

                string heroKey = HERO_MAP + item.HERO.AsByteArray();
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                if (heroBytes.Length <= 0)
                {
                    Runtime.Log("ITEM_SELLER_MUST_BE_ON_BLOCKCHAIN");
                    throw new Exception();
                }

                // Seller
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
                if (Runtime.CheckWitness(hero.OWNER))
                {
                    Runtime.Log("ITEM_BUYER_MUST_BE_NOT_SELLER");
                    throw new Exception();
                }

                string buyerHeroKey = HERO_MAP + heroId.AsByteArray();
                byte[] buyerHeroBytes = Storage.Get(Storage.CurrentContext, buyerHeroKey);
                if (buyerHeroBytes.Length <= 0)
                {
                    Runtime.Log("ITEM_BUYER_MUST_BE_ON_BLOCKCHAIN");
                    throw new Exception();
                }

                Hero buyer = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(buyerHeroBytes);
                if (!Runtime.CheckWitness(buyer.OWNER))
                {
                    Runtime.Log("BUYER_HERO_MUST_BE_VALID");
                    throw new Exception();
                }

                string marketItemKey = MARKET_MAP + itemId.AsByteArray();
                byte[] marketItemBytes = Storage.Get(Storage.CurrentContext, marketItemKey);
                if (marketItemBytes.Length > 0)
                {
                    MarketItemData marketItem = (MarketItemData)Neo.SmartContract.Framework.Helper.Deserialize(marketItemBytes);
                    if (marketItem.Duration + marketItem.CreatedTime >= Blockchain.GetBlock(Blockchain.GetHeight()).Timestamp)
                    {
                        Runtime.Log("ITEM_MUST_BE_ON_MARKET");
                        throw new Exception();
                    }
                    else
                    {
                        Runtime.Notify("Market item duration is not expired, duration and created time", marketItem.Duration, marketItem.CreatedTime);

                        /// original price based sum of money that buyer should attach to tx.
                        byte[] totalPricePercentsBytes = Storage.Get(Storage.CurrentContext, PERCENTS_PURCHACE);
                        BigInteger totalPricePercents = totalPricePercentsBytes.AsBigInteger();

                        BigInteger pricePercent = BigInteger.Divide(marketItem.Price, 100);
                        BigInteger totalPrice = BigInteger.Multiply(pricePercent, totalPricePercents);

                        byte[] lordPercentsBytes = Storage.Get(Storage.CurrentContext, PERCENTS_LORD);
                        BigInteger lordPercents = lordPercentsBytes.AsBigInteger();

                        BigInteger gameOwnerExpectation = 0;
                        BigInteger lordExpectation = 0;
                        BigInteger sellerExpectation = marketItem.Price;

                        string cityKey = CITY_MAP + marketItem.City.AsByteArray();
                        City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, cityKey));
                        if (city.Hero > 0 && city.Hero == heroId)
                        {
                            sellerExpectation = 0;
                            lordExpectation = BigInteger.Add(marketItem.Price, BigInteger.Multiply(pricePercent, lordPercents));
                        }
                        else if (city.Hero > 0)
                        {
                            lordExpectation = BigInteger.Multiply(pricePercent, lordPercents);
                        }

                        /// All additional GAS over original price goes to Game Owner
                        gameOwnerExpectation = BigInteger.Subtract(totalPrice, marketItem.Price);
                        if (city.Hero <= 0)
                        { 
                            // City lord exists? Game owner can not pretend to lord's fee!
                            gameOwnerExpectation = BigInteger.Subtract(gameOwnerExpectation, BigInteger.Multiply(pricePercent, lordPercents));
                        }

                        if (sellerExpectation > 0 && !AttachmentExist(sellerExpectation, hero.OWNER))
                        {
                            Runtime.Log("ITEM_SELLER_MUST_GET_CORRECT_GAS_AMOUNT");
                            throw new Exception();
                        }

                        if (city.Hero > 0)
                        {
                            string cityLordKey = HERO_MAP + city.Hero.AsByteArray();
                            Hero cityLord = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, cityLordKey));
                            if (lordExpectation > 0 && !AttachmentExist(lordExpectation, cityLord.OWNER))
                            {
                                Runtime.Log("CITY_LORD_MUST_GET_CORRECT_GAS_AMOUNT");
                                throw new Exception();
                            }
                        }

                        if (gameOwnerExpectation > 0 && !AttachmentExist(gameOwnerExpectation, GameOwner))
                        {
                            Runtime.Log("GAME_CREATERS_MUST_GET_CORRECT_GAS_AMOUNT");
                            throw new Exception();
                        }

                        city.ItemsOnMarket = BigInteger.Subtract(city.ItemsOnMarket, 1);
                        Storage.Put(Storage.CurrentContext, cityKey, Neo.SmartContract.Framework.Helper.Serialize(city));

                        item.HERO = heroId;
                        Storage.Put(Storage.CurrentContext, itemKey, Neo.SmartContract.Framework.Helper.Serialize(item));

                        Storage.Delete(Storage.CurrentContext, marketItemKey);

                        
                    }
                }
                return new BigInteger(0).AsByteArray();
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
