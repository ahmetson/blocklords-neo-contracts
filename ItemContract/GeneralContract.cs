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
        public static readonly string INTERVAL_DROP = "\x30";
        /// <summary>
        /// Tracks strongholds amount on Contract
        /// </summary>
        public static readonly string AMOUNT_STRONGHOLDS = "\x31";
        /// <summary>
        /// Tracks cities amount on Contract
        /// </summary>
        public static readonly string AMOUNT_CITIES = "\x32";
        /// <summary>
        /// Storage prefix for bandit points
        /// </summary>
        public static readonly string BANDIT_CAMP_MAP = "\x33\x00";
        /// <summary>
        /// Battle Log Prefix
        /// </summary>
        public static readonly string BATTLE_LOG_MAP = "\x34\x00";
        /// <summary>
        /// Amount of Bandit camps
        /// </summary>
        public static readonly string AMOUNT_BATTLE_CAMP = "\x35";
        /// <summary>
        /// Coffer Payment Map
        /// </summary>
        public static readonly string COFFER_PAYMENT_SESSION_MAP = "\x36\x00";
        /// <summary>
        /// Last Session
        /// </summary>
        public static readonly string COFFER_PAYMENT_SESSION = "\x37";
        /// <summary>
        /// Stronghold Reward Log
        /// </summary>
        public static readonly string STRONGHOLD_REWARD_MAP = "\x38\x00";
        /// <summary>
        /// Latest Stronghold Reward
        /// </summary>
        public static readonly string LAST_ITEM_DROP = "\x39";
        /// <summary>
        /// Range in Which coffer from transaction fee could go
        /// </summary>
        public static readonly BigInteger PERCENTS_PVC_COFFER_MAX = 70;
        public static readonly BigInteger PERCENTS_PVC_COFFER_MIN = 0;


        /// <summary>
        /// Battle type
        /// </summary>
        public static readonly BigInteger PVC = 1, PVP = 0, PVE = 2;

        /// <summary>
        /// Item batch type
        /// </summary>
        public static readonly byte HERO_CREATION_BATCH = 1, STRONGHOLD_REWARD_BATCH = 0, NO_BATCH = 2;

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
        //public static readonly byte[] GameOwner = "ARxEMtapvYPp6ACc5P86WHSZPeVzgoB18r".ToScriptHash();//"AML8hyTV4vXuomovxdcAH9pRC9ny618YmA".ToScriptHash();
        public static readonly byte[] GameOwnerPublicKey = "031a6c6fbbdf02ca351745fa86b9ba5a9452d785ac4f7fc2b7548ca2a46c4fcf4a".HexToBytes();//"AML8hyTV4vXuomovxdcAH9pRC9ny618YmA".ToScriptHash();

        /// <summary>
        /// City type
        /// </summary>
        public static readonly BigInteger CITY_TYPE_BIG = 1, CITY_TYPE_MID = 2, CITY_TYPE_SMALL = 3;

        /// <summary>
        /// Battle result type
        /// </summary>
        public static readonly BigInteger ATTACKER_WON = 1, ATTACKER_LOSE = 2;

        /// Event Hero Creation
        //[DisplayName("heroCreation")]
        //public static event Action<BigInteger, byte[], BigInteger[], BigInteger[]> heroCreation;

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
                return Settings.Set((string)args[0], (BigInteger)args[1]);
            }
            else if (param.Equals("payoutCityCoffer"))
            {
                return Periodical.PayCityCoffer((BigInteger)args[0]);
            }
            else if (param.Equals("dropItems"))
            {
                return Periodical.SimpleDropItems((byte[])args[0]);
            }
            else if (param.Equals("putCity"))
            {
                return Put.City((BigInteger)args[0], (BigInteger)args[1], (BigInteger)args[2]);
            }
            else if (param.Equals("putStronghold"))
            {
               
                return Put.Stronghold((BigInteger)args[0]);
            }
            else if (param.Equals("putBanditCamp"))
            {
                return Put.BanditCamp((BigInteger)args[0]);
            }
            else if (param.Equals("putItem"))
            {
                if (args.Length != 6)
                {
                    Runtime.Notify(1001); // This function has 7 parameters
                    throw new Exception();
                }

                // Item given type: for hero creation or drop for stronghold
                if ((BigInteger)args[0] != STRONGHOLD_REWARD_BATCH && (BigInteger)args[0] != HERO_CREATION_BATCH)
                {
                    Runtime.Notify(1002);
                    throw new Exception();
                }

                Runtime.Log("Item was upload");

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

                Runtime.Log("Item settings were set");

                Put.Item((byte[])args[1], item, false);
            }
            else if (param.Equals("putHero"))
            {
                if (Runtime.CheckWitness(GameOwner))
                {
                    Runtime.Notify(1);
                    throw new Exception();
                }


                byte[] scriptHash = (byte[])args[0];
                if (!Runtime.CheckWitness(scriptHash))
                {
                    Runtime.Notify(4002);
                    throw new Exception();
                }

                BigInteger heroId = (BigInteger)args[1];
                BigInteger refererHeroId = (BigInteger)args[2];
                byte[] refererScriptHash = (byte[])args[3];

                if (heroId <= 0)
                {
                    Runtime.Notify(4003);
                    throw new Exception();
                }

                string heroKey = HERO_MAP + heroId.AsByteArray();
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                if (heroBytes.Length > 0)
                {
                    Runtime.Notify(4004);
                    throw new Exception();
                }

                if (refererHeroId > 0)
                {
                    string refererHeroKey = HERO_MAP + refererHeroId.AsByteArray();
                    byte[] refererHeroBytes = Storage.Get(Storage.CurrentContext, refererHeroKey);
                    if (refererHeroBytes.Length <= 0)
                    {
                        Runtime.Notify(4005);
                        throw new Exception();
                    }

                    Hero refererHero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(refererHeroBytes);

                    if (Runtime.CheckWitness(refererHero.OWNER))
                    {
                        Runtime.Notify(4006);
                        throw new Exception();
                    }

                    if (refererHero.OWNER.AsBigInteger() != refererScriptHash.AsBigInteger())
                    {
                        Runtime.Notify(4007);
                        throw new Exception();
                    }

                    byte[] feeBytes = Storage.Get(Storage.CurrentContext, FEE_HERO_CREATION);
                    BigInteger fee = feeBytes.AsBigInteger();

                    byte[] refererBytes = Storage.Get(Storage.CurrentContext, FEE_REFERAL);
                    BigInteger refererFee = refererBytes.AsBigInteger();

                    // Game owner's fee is less, if we have a referer
                    fee = BigInteger.Subtract(fee, refererFee);

                    if (!AttachmentExist(refererFee, refererHero.OWNER))
                    {
                        Runtime.Notify(4008);
                        throw new Exception();
                    }

                    if (!AttachmentExist(fee, GameOwner))
                    {
                        Runtime.Notify(4009);
                        throw new Exception();
                    }
                }
                else
                {
                    byte[] feeBytes = Storage.Get(Storage.CurrentContext, FEE_HERO_CREATION);
                    BigInteger fee = feeBytes.AsBigInteger();
                    if (!AttachmentExist(fee, GameOwner))
                    {
                        Runtime.Notify(4009);
                        throw new Exception();
                    }

                }

                BigInteger[] stats = (BigInteger[])args[4]; 
                if (stats.Length != 5)
                {
                    Runtime.Notify(4010);
                    throw new Exception();
                }

                Hero hero3 = new Hero();
                hero3.OWNER = scriptHash;
                hero3.INTELLIGENCE = stats[2];
                hero3.SPEED = stats[3];
                hero3.STRENGTH = stats[4];
                hero3.LEADERSHIP = stats[1];
                hero3.DEFENSE = stats[0];
                hero3.StrongholsAmount = 0;

                byte[] equipment1 = (byte[])args[5];
                byte[] equipment2 = (byte[])args[6];
                byte[] equipment3 = (byte[])args[7];
                byte[] equipment4 = (byte[])args[8];
                byte[] equipment5 = (byte[])args[9];

                // Change Item Owners
                Helper.ChangeItemOwner(equipment1, heroId);
                Helper.ChangeItemOwner(equipment2, heroId);
                Helper.ChangeItemOwner(equipment3, heroId);
                Helper.ChangeItemOwner(equipment4, heroId);
                Helper.ChangeItemOwner(equipment5, heroId);

                byte[] result = Put.Hero(heroId, hero3);

                //heroCreation(heroId, scriptHash, stats, equipments);

                Runtime.Notify(4000, scriptHash, heroId, refererHeroId, refererScriptHash, stats, equipment1, equipment2, equipment3, equipment4, equipment5);
                
                return result;
            }
            else if (param.Equals("marketAddItem"))
            {
                // 1: Hero Id, 2: Item Id, 3: Price, 4: Duration in seconds, 5: City ID
                if (args.Length != 5)
                {
                    Runtime.Notify(1001);
                    return new BigInteger(0).AsByteArray();
                }

                byte[] itemId = (byte[])args[0];
                BigInteger price = (BigInteger)args[1];
                BigInteger duration = (BigInteger)args[2];
                BigInteger cityId = (BigInteger)args[3];

                if (Runtime.CheckWitness(GameOwner))
                {
                    Runtime.Notify(1);
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
                    Runtime.Notify(1002);
                    throw new Exception();
                }

                string cityKey = CITY_MAP + cityId.AsByteArray();
                byte[] cityBytes = Storage.Get(Storage.CurrentContext, cityKey);
                if (cityBytes.Length <= 0)
                {
                    Runtime.Notify(1003);
                    throw new Exception();
                }

                City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);
                
                if (city.ItemsOnMarket >= city.ItemsCap)
                {
                    Runtime.Notify(1004);
                    throw new Exception();
                }

                string itemKey = ITEM_MAP + itemId;
                byte[] itemBytes = Storage.Get(Storage.CurrentContext, itemKey);

                if (itemBytes.Length <= 0)
                {
                    Runtime.Notify(1005);
                    throw new Exception();
                }

                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes);
                if (item.HERO <= 0)
                {
                    Runtime.Notify(1006);
                    throw new Exception();
                }

                string heroKey = HERO_MAP + item.HERO.AsByteArray();
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                if (heroBytes.Length <= 0)
                {
                    Runtime.Notify(1007);
                    throw new Exception();
                }

                // Seller
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
                if (!Runtime.CheckWitness(hero.OWNER))
                {
                    Runtime.Notify(1008);
                    throw new Exception();
                }

                string marketItemKey = MARKET_MAP + itemId;
                byte[] marketItemBytes = Storage.Get(Storage.CurrentContext, marketItemKey);
                if (marketItemBytes.Length > 0)
                {
                    MarketItemData oldMarketItem = (MarketItemData)Neo.SmartContract.Framework.Helper.Deserialize(marketItemBytes);
                    if (Blockchain.GetBlock(Blockchain.GetHeight()).Timestamp < oldMarketItem.Duration + oldMarketItem.CreatedTime)
                    {
                        Runtime.Notify(1009);
                        throw new Exception();
                    }

                }

                BigInteger durationFee = durationFeeBytes.AsBigInteger();
                if (!AttachmentExist(durationFee, GameOwner))
                {
                    Runtime.Notify(1010);
                    throw new Exception();
                }

                MarketItemData marketItem = new MarketItemData();
                marketItem.Duration = duration;
                marketItem.Price = price;
                marketItem.City = cityId;
                marketItem.CreatedTime = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;
                marketItem.Seller = hero.OWNER;

                marketItemBytes = Neo.SmartContract.Framework.Helper.Serialize(marketItem);

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

                Runtime.Notify(1000, itemId, price, duration, cityId);
                return new BigInteger(0).AsByteArray();
            }
            else if (param.Equals("marketBuyItem"))
            {
                BigInteger heroId = (BigInteger)args[0];
                byte[] itemId = (byte[])args[1];

                if (Runtime.CheckWitness(GameOwner))
                {
                    Runtime.Notify(1);
                    throw new Exception();
                }

                string itemKey = ITEM_MAP + itemId;
                byte[] itemBytes = Storage.Get(Storage.CurrentContext, itemKey);

                if (itemBytes.Length <= 0)
                {
                    Runtime.Notify(1005);
                    throw new Exception();
                }

                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes);
                if (item.HERO <= 0)
                {
                    Runtime.Notify(1006);
                    throw new Exception();
                }

                string heroKey = HERO_MAP + item.HERO.AsByteArray();
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                if (heroBytes.Length <= 0)
                {
                    Runtime.Notify(1007);
                    throw new Exception();
                }

                // Seller
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
                if (Runtime.CheckWitness(hero.OWNER))
                {
                    Runtime.Notify(2004);
                    throw new Exception();
                }

                string buyerHeroKey = HERO_MAP + heroId.AsByteArray();
                byte[] buyerHeroBytes = Storage.Get(Storage.CurrentContext, buyerHeroKey);
                if (buyerHeroBytes.Length <= 0)
                {
                    Runtime.Notify(2005);
                    throw new Exception();
                }

                Hero buyer = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(buyerHeroBytes);
                if (!Runtime.CheckWitness(buyer.OWNER))
                {
                    Runtime.Notify(2006);
                    throw new Exception();
                }

                string marketItemKey = MARKET_MAP + itemId;
                byte[] marketItemBytes = Storage.Get(Storage.CurrentContext, marketItemKey);
                if (marketItemBytes.Length > 0)
                {
                    MarketItemData marketItem = (MarketItemData)Neo.SmartContract.Framework.Helper.Deserialize(marketItemBytes);
                    if (Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp > marketItem.Duration + marketItem.CreatedTime)
                    {
                        Runtime.Notify(2007);
                        throw new Exception();
                    }
                    else
                    {
                        /// original price based sum of money that buyer should attach to tx.
                        byte[] totalPricePercentsBytes = Storage.Get(Storage.CurrentContext, PERCENTS_PURCHACE);
                        BigInteger totalPricePercents = totalPricePercentsBytes.AsBigInteger();

                        string cityKey = CITY_MAP + marketItem.City.AsByteArray();
                        City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, cityKey));

                        BigInteger gameOwnerExpectation = 0;
                        BigInteger lordExpectation = 0;
                        BigInteger sellerExpectation = marketItem.Price;

                        if (totalPricePercents > 0)
                        {

                            BigInteger pricePercent = BigInteger.Divide(marketItem.Price, 100);
                            BigInteger totalPrice = BigInteger.Multiply(pricePercent, totalPricePercents);

                            byte[] lordPercentsBytes = Storage.Get(Storage.CurrentContext, PERCENTS_LORD);
                            BigInteger lordPercents = lordPercentsBytes.AsBigInteger();

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
                            if (city.Hero > 0)
                            {
                                // City lord exists? Game owner can not pretend to lord's fee!
                                gameOwnerExpectation = BigInteger.Subtract(gameOwnerExpectation, BigInteger.Multiply(pricePercent, lordPercents));
                            }

                            if (sellerExpectation > 0 && !AttachmentExist(sellerExpectation, hero.OWNER))
                            {
                                Runtime.Notify(2008);
                                throw new Exception();
                            }

                            if (city.Hero > 0)
                            {
                                string cityLordKey = HERO_MAP + city.Hero.AsByteArray();
                                Hero cityLord = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, cityLordKey));
                                if (lordExpectation > 0 && !AttachmentExist(lordExpectation, cityLord.OWNER))
                                {
                                    Runtime.Notify(2009);
                                    throw new Exception();
                                }
                            }

                            if (gameOwnerExpectation > 0 && !AttachmentExist(gameOwnerExpectation, GameOwner))
                            {
                                Runtime.Notify(2010);
                                throw new Exception();
                            }

                        }
                        city.ItemsOnMarket = BigInteger.Subtract(city.ItemsOnMarket, 1);
                        Storage.Put(Storage.CurrentContext, cityKey, Neo.SmartContract.Framework.Helper.Serialize(city));

                        item.HERO = heroId;
                        Storage.Put(Storage.CurrentContext, itemKey, Neo.SmartContract.Framework.Helper.Serialize(item));

                        Storage.Delete(Storage.CurrentContext, marketItemKey);

                        Runtime.Notify(2000, itemId, heroId, sellerExpectation, lordExpectation, gameOwnerExpectation);
                    }
                }
                else
                {
                    Runtime.Notify(2000);
                    throw new Exception();
                }
                return new BigInteger(0).AsByteArray();
            }
            else if (param.Equals("marketDeleteItem"))
            {
                byte[] itemId = (byte[])args[0];

                if (Runtime.CheckWitness(GameOwner))
                {
                    Runtime.Notify(1);
                    throw new Exception();
                }

                string key = MARKET_MAP + itemId;
                byte[] mBytes = Storage.Get(Storage.CurrentContext, key);
                if (mBytes.Length <= 0)
                {
                    Runtime.Notify(2011);
                    throw new Exception();
                }

                MarketItemData mItem = (MarketItemData)Neo.SmartContract.Framework.Helper.Deserialize(mBytes);

                if (!Runtime.CheckWitness(mItem.Seller))
                {
                    Runtime.Notify(3002);
                    throw new Exception();
                }

                string cityKey = CITY_MAP + mItem.City.AsByteArray();
                byte[] cityBytes = Storage.Get(Storage.CurrentContext, cityKey);

                City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);
                city.ItemsOnMarket = BigInteger.Subtract(city.ItemsOnMarket, 1);

                Storage.Delete(Storage.CurrentContext, key);

                cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
                Storage.Put(Storage.CurrentContext, cityKey, cityBytes);

                Runtime.Notify(3000, itemId);
                return new BigInteger(1).AsByteArray();
            }
            else if (param.Equals("logBattle"))
            {
                return Log.Battle(args);
            }

            return new BigInteger(1).AsByteArray();
        }

        /// <summary>
        /// Retrieve pseudo random number
        /// </summary>
        /// <param name="min">Min range</param>
        /// <param name="max">Max range</param>
        /// <returns>generated number</returns>
        public static BigInteger GetRandomNumber(BigInteger min, BigInteger max)
        {
            byte[] salt = Gen();
            byte[] rand = Ran(salt, 6);
            BigInteger randBig = rand.AsBigInteger();

            return randBig % max;
        }

        public static byte[] Ran(byte[] salt, int size = 1)
        {
            Transaction tx = (Transaction)ExecutionEngine.ScriptContainer;
            Header bl = Blockchain.GetHeader(Blockchain.GetHeight());
            return Hash256(bl.Hash.Concat(tx.Hash).Concat(salt)).Range(0, size);
        }

        public static byte[] Gen(int size = 5)
        {
            byte[] zero = new byte[0];
            return Ran(zero, size);
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

            foreach (var output in outputs)
            {
                // Game Developers got their fee?
                if (output.ScriptHash.AsBigInteger() == GeneralContract.GameOwner.AsBigInteger())
                {

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
            if (value <= 0)
                return true;
            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();

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
