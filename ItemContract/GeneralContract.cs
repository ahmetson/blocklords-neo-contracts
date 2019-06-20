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
        ///// <summary>
        ///// Storage Key of Refering Fee
        ///// </summary>
        //public static readonly string FEE_REFERAL = "\x17";
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
        public static readonly string PERCENTS_GAME_OWNER = "\x24";
        /// <summary>
        /// Storage Key of GAS in percents that lord of a city should get from buyer.
        /// As a base sum for calculation of GAS in percents that lord of a city should get is used market item price
        /// </summary>
        public static readonly string PERCENTS_LORD = "\x25";
        /// <summary>
        /// Storage Key of GAS attachments in percents of City Attacks,
        /// that will be transfered to city coffer  
        /// </summary>
        public static readonly string PVC_COFFER_ADDITION_AMOUNT = "\x27";
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
        /// Storage Key of GAS in percents that city coffer will get for every added market item.
        /// </summary>
        public static readonly string MARKET_COFFER_ADDITION_8_HOURS = "\x26";
        public static readonly string MARKET_COFFER_ADDITION_12_HOURS = "\x40";
        public static readonly string MARKET_COFFER_ADDITION_24_HOURS = "\x41";
        /// <summary>
        /// Storage key for City coffers
        /// </summary>
        public static readonly string CITY_COFFERS_KEY = "\x42";
        /// <summary>
        /// Max amount of cities
        /// </summary>
        public static readonly BigInteger MAX_CITY_AMOUNT = 101;    // Substract 1 from Max amount.

        /// <summary>
        /// Battle type
        /// </summary>
        public static readonly BigInteger PVC = 1, PVP = 0, PVE = 2;

        /// <summary>
        /// Item batch type
        /// </summary>
        public static readonly byte HERO_CREATION_BATCH = 1, STRONGHOLD_REWARD_BATCH = 0, NO_BATCH = 2;

        public static readonly BigInteger duration8Hours = 28800, duration12Hours = 43200, duration24Hours = 86400;                 // 86_400 Seconds are 24 hours

        /// <summary>
        /// Game Owner's script hash: 1. Privatenet included in Neo-local, 2. Testnet 3. mainnet
        /// </summary>
        //public static readonly byte[] GameOwner = "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y".ToScriptHash();//"AML8hyTV4vXuomovxdcAH9pRC9ny618YmA".ToScriptHash();
        //public static readonly byte[] GameOwnerPublicKey = "031a6c6fbbdf02ca351745fa86b9ba5a9452d785ac4f7fc2b7548ca2a46c4fcf4a".HexToBytes();//"AML8hyTV4vXuomovxdcAH9pRC9ny618YmA".ToScriptHash();

        public static readonly byte[] GameOwner = "ARxEMtapvYPp6ACc5P86WHSZPeVzgoB18r".ToScriptHash();//"AML8hyTV4vXuomovxdcAH9pRC9ny618YmA".ToScriptHash();
        public static readonly byte[] GameOwnerPublicKey = "021c2ca353f94e810b315180ba46a3c6140c1804a63066a36007f2b46b01d67261".HexToBytes();//"AML8hyTV4vXuomovxdcAH9pRC9ny618YmA".ToScriptHash();

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
            if (param.Equals("initializeCoffers"))
            {
                int maxCityAmount = (int)MAX_CITY_AMOUNT;
                BigInteger[] coffersList = new BigInteger[maxCityAmount];

                int i = 0;
                for (; i<maxCityAmount; i++)
                {
                    coffersList[i] = 0;
                }

                byte[] coffers = Neo.SmartContract.Framework.Helper.Serialize(coffersList);
                Storage.Put(CITY_COFFERS_KEY, coffers);
            }
            else if (param.Equals("setSetting"))
            {
                Settings.Set((string)args[0], (byte[])args[1]);
            }
            else if (param.Equals("payoutCoffers"))
            {
                // Check that coffer percent parameter is matching with setting value
                byte[] cofferPercentsSettingBytes = Storage.Get(Storage.CurrentContext, PERCENTS_COFFER_PAY);
                byte[] cofferPercentsBytes = (byte[])args[1];
                BigInteger payoutPercents = (BigInteger)args[1];
                if (!cofferPercentsSettingBytes.Equals(cofferPercentsBytes))
                {
                    Runtime.Notify(11);
                    throw new Exception();
                }

                BigInteger payoutMin = 1;
                BigInteger payoutMax = 99;
                if (payoutPercents > 5)
                {
                    payoutMin = BigInteger.Subtract(payoutPercents, 5);
                }
                if (payoutPercents < 95)
                {
                    payoutMax = BigInteger.Add(payoutPercents, 5);
                }

                // Check that payment interval parameter is matching with setting value
                byte[] paymentIntervalSettingBytes = Storage.Get(Storage.CurrentContext, GeneralContract.INTERVAL_COFFER);
                byte[] paymentIntervalBytes = (byte[])args[2];
                BigInteger paymentInterval = (BigInteger)args[2];
                if (!paymentIntervalSettingBytes.Equals(paymentIntervalBytes))
                {
                    Runtime.Notify(10);
                    throw new Exception();
                }

                // Get latest coffer payout
                CofferPayment session = new CofferPayment();
                session.Block = 1;// Blockchain.GetHeight();
                byte[] lastCofferSession = Storage.Get(Storage.CurrentContext, COFFER_PAYMENT_SESSION);
                if (lastCofferSession.Length > 0)
                {
                    session = (CofferPayment)Neo.SmartContract.Framework.Helper.Deserialize(lastCofferSession);
                }
                if (Blockchain.GetHeight() < session.Block + paymentInterval)
                {
                    Runtime.Notify(6001);
                    throw new Exception();
                }

                // Get city amount
                byte[] cityAmountSettingBytes = Storage.Get(AMOUNT_CITIES);
                if (cityAmountSettingBytes.Length <= 0)
                {
                    Runtime.Notify(15);
                    throw new Exception();
                }
                byte[] cityAmountBytes = (byte[])args[0];
                if (!cityAmountBytes.Equals(cityAmountSettingBytes))
                {
                    Runtime.Notify(9);
                    throw new Exception();
                }
                int cityAmountInt = (int)args[0];

                // We track the checked outputs to prevent of using the same outputs for the lord,
                // when lord has many cities with same amount of coffers
                int[] outputIndex = new int[cityAmountInt];
                int found = 0;

                Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
                TransactionOutput[] outputs = TX.GetOutputs();

                BigInteger[] coffers = Helper.GetCoffers();

                BigInteger cityId = 1;
                for (var id = 1; id <= cityAmountInt; id++, cityId = BigInteger.Add(cityId, 1))
                {
                    BigInteger coffer = coffers[id];
                    // Skip from paying out if number is less than 0.01 GAS
                    if (coffer <= 1000000)
                    {
                        continue;
                    }

                    BigInteger percent = BigInteger.Divide(coffer, 100);
                    BigInteger payoutAmount = BigInteger.Multiply(percent, payoutPercents);

                    BigInteger payoutMinAmount = BigInteger.Multiply(percent, payoutMin);
                    BigInteger payoutMaxAmount = BigInteger.Multiply(percent, payoutMax);

                    //    get cityData
                    byte[] cityIdBytes = cityId.ToByteArray();
                    string cityKey = CITY_MAP + cityIdBytes;
                    byte[] cityBytes = Storage.Get(Storage.CurrentContext, cityKey);
                    if (cityBytes.Length <= 0)
                    {
                        Runtime.Notify(1003);
                        throw new Exception();
                    }

                    // Get city lord
                    City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);
                    if (city.Hero > 0)
                    {
                        BigInteger cityLordId = city.Hero;
                        string heroKey = HERO_MAP + cityLordId.ToByteArray();
                        byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                        Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
                        byte[] lord = hero.OWNER;

                        // Check the coffer payout
                        bool outputValid = false;
                        for (var i = 0; i < outputs.Length; i++)
                        {
                            // Skip output if it's value was used before
                            bool outputUsed = false;
                            for (int j = 0; j < found; j++)
                            {
                                if (outputIndex[j] == i + 1)
                                {
                                    outputUsed = true;
                                    break;
                                }
                            }
                            if (outputUsed)
                                continue;

                            // Check output
                            long outputVal = outputs[i].Value;
                            if (outputs[i].ScriptHash.Equals(lord))
                            {
                                if (outputVal >= payoutMinAmount && outputVal <= payoutMaxAmount)
                                {
                                    outputIndex[found] = i + 1;
                                    found++;
                                    outputValid = true;
                                    coffers[id] = BigInteger.Subtract(coffers[id], outputVal);
                                }
                                

                            }
                        }
                        if (!outputValid)
                        {
                            Runtime.Notify(6005, payoutAmount, coffer, cityId, payoutPercents);
                        }
                    }
                    else
                    {
                        coffers[id] = BigInteger.Subtract(coffers[id], payoutAmount);
                    }
                }

                Helper.SetCoffers(coffers);
                session.Block = Blockchain.GetHeight();
                lastCofferSession = Neo.SmartContract.Framework.Helper.Serialize(session);
                Storage.Put(Storage.CurrentContext, COFFER_PAYMENT_SESSION, lastCofferSession);
                Runtime.Notify(6000);
            }
            else if (param.Equals("dropItem"))
            {
                Periodical.SimpleDropItem((byte[])args[0], args[1], args[2]);
            }
            else if (param.Equals("putCity"))
            {
                Put.City((BigInteger)args[0], (BigInteger)args[1], (BigInteger)args[2]);
            }
            else if (param.Equals("putStronghold"))
            {
               
                Put.Stronghold((BigInteger)args[0]);
            }
            else if (param.Equals("putBanditCamp"))
            {
                Put.BanditCamp((BigInteger)args[0]);
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

                Runtime.Log("Game Admin is not running game. Check scripthash");
                byte[] scriptHash = (byte[])args[0];
                if (!Runtime.CheckWitness(scriptHash))
                {
                    Runtime.Notify(4002);
                    throw new Exception();
                }

                Runtime.Log("Scripthash wallet is running game. Check referers");

                BigInteger heroId = (BigInteger)args[1];
                BigInteger refererHeroId = (BigInteger)args[2];
                byte[] refererScriptHash = (byte[])args[3];

                Runtime.Log("Referers were checked. Check Hero ID");

                if (heroId <= 0)
                {
                    Runtime.Notify(4003);
                    throw new Exception();
                }

                Runtime.Log("Hero id is not 0 or lower than 0. Check existance of hero on the given id");

                string heroKey = HERO_MAP + heroId.ToByteArray();
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                if (heroBytes.Length > 0)
                {
                    Runtime.Notify(4004);
                    throw new Exception();
                }

                Runtime.Log("Hero on the id is not existing. Check the Referer hero");

                if (refererHeroId > 0)
                {
                    Runtime.Log("Referer is exist");

                    string refererHeroKey = HERO_MAP + refererHeroId.ToByteArray();
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

                    if (!refererHero.OWNER.Equals(refererScriptHash))
                    {
                        Runtime.Notify(4007);
                        throw new Exception();
                    }
                }
                
                
                Runtime.Log("Referer is not exist");

                byte[] feeBytes = Storage.Get(Storage.CurrentContext, FEE_HERO_CREATION);

                if (!AttachmentExistAB(feeBytes, GameOwner))
                {
                    Runtime.Notify(4009);
                    throw new Exception();
                }
                Runtime.Log("Hero creation fee is attached");


                byte[][] stats = (byte[][])args[4];

                Runtime.Log("Array of stats are not given");

                if (stats.Length != 5)
                {
                    Runtime.Notify(4010);
                    throw new Exception();
                }

                Runtime.Log("Array of stats are given");

                Hero hero = new Hero();
                hero.OWNER = scriptHash;
                hero.INTELLIGENCE = stats[2];
                hero.SPEED = stats[3];
                hero.STRENGTH = stats[4];
                hero.LEADERSHIP = stats[1];
                hero.DEFENSE = stats[0];
                hero.StrongholdsAmount = 0;
                hero.ID = heroId;

                Runtime.Log("Hero Data set");

                byte[] equipment1 = (byte[])args[5];
                byte[] equipment2 = (byte[])args[6];
                byte[] equipment3 = (byte[])args[7];
                byte[] equipment4 = (byte[])args[8];
                byte[] equipment5 = (byte[])args[9];

                Runtime.Log("Define equipments");
                
                byte[] signature = (byte[])args[10];

                Runtime.Log("Define signature");

                byte[] heroIdBytes = heroId.ToByteArray();

                Runtime.Log("Hero Id converted to bytes");

                Runtime.Log("Define sign message");

                byte[] signMessage = Neo.SmartContract.Framework.Helper.Concat(heroIdBytes, stats[0]);

                Runtime.Log("Concatination of Defense was succ");

                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, hero.LEADERSHIP);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, hero.INTELLIGENCE);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, hero.SPEED);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, hero.STRENGTH);

                Runtime.Log("Concatination of all stats was successfull");

                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, equipment1);

                Runtime.Log("Concatination of equpiment id was successfull");

                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, equipment2);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, equipment3);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, equipment4);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, equipment5);

                Runtime.Log("Concatted Item");

                Runtime.Log("Message converted");

                if (!VerifySignature(signMessage, signature, GameOwnerPublicKey))
                {
                    Runtime.Notify(2);
                    Runtime.Notify(signMessage);
                    Runtime.Notify(signature);
                    Runtime.Log("Signature Verification Failed");
                    throw new Exception();
                }

                Runtime.Log("Verified successuly");

                // Change Item Owners
                Helper.ChangeItemOwner(equipment1, heroId);
                Helper.ChangeItemOwner(equipment2, heroId);
                Helper.ChangeItemOwner(equipment3, heroId);
                Helper.ChangeItemOwner(equipment4, heroId);
                Helper.ChangeItemOwner(equipment5, heroId);


                Put.Hero(heroId, hero);

                //heroCreation(heroId, scriptHash, stats, equipments);

                Runtime.Notify(4000, scriptHash, heroId, refererHeroId, refererScriptHash, stats, equipment1, equipment2, equipment3, equipment4, equipment5);
            }
            else if (param.Equals("marketAddItem"))
            {
                // 1: Item Id, 2: Price, 3: Duration in seconds, 4: City ID, 5: coffer addition amount
                if (args.Length != 5)
                {
                    Runtime.Notify(1001);
                    throw new Exception();
                }

                Runtime.Log("Market enetered");

                byte[] itemId = (byte[])args[0];
                BigInteger price = (BigInteger)args[1];
                BigInteger duration = (BigInteger)args[2];
                BigInteger cityId = (BigInteger)args[3];
                byte[] cofferAdditionAmountBytes = (byte[])args[4];

                Runtime.Log("Item data retrieved");

                if (Runtime.CheckWitness(GameOwner))
                {
                    Runtime.Notify(1);
                    throw new Exception();
                }

                Runtime.Log("Data is not called by player");

                byte[] durationFeeSettingBytes = new byte[0] { };
                // Increase coffer
                byte[] marketCofferAdditionSettingBytes = new byte[0] { };
                if (duration == duration8Hours)
                {
                    durationFeeSettingBytes = Storage.Get(Storage.CurrentContext, FEE_8_HOURS);
                    marketCofferAdditionSettingBytes = Storage.Get(Storage.CurrentContext, MARKET_COFFER_ADDITION_8_HOURS);
                }
                else if (duration == duration12Hours)
                {
                    durationFeeSettingBytes = Storage.Get(Storage.CurrentContext, FEE_12_HOURS);
                    marketCofferAdditionSettingBytes = Storage.Get(Storage.CurrentContext, MARKET_COFFER_ADDITION_12_HOURS);
                }
                else if (duration == duration24Hours)
                {
                    durationFeeSettingBytes = Storage.Get(Storage.CurrentContext, FEE_24_HOURS);
                    marketCofferAdditionSettingBytes = Storage.Get(Storage.CurrentContext, MARKET_COFFER_ADDITION_24_HOURS);
                }
                else
                {
                    Runtime.Notify(1002);
                    throw new Exception();
                }

                Runtime.Log("Duration is retreived from storage");

                string cityKey = CITY_MAP + cityId.ToByteArray();
                byte[] cityBytes = Storage.Get(Storage.CurrentContext, cityKey);
                if (cityBytes.Length <= 0)
                {
                    Runtime.Notify(1003);
                    throw new Exception();
                }

                Runtime.Log("City on storage");

                City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);

                if (city.ItemsOnMarket + 1 >= city.ItemsCap)
                {
                    Runtime.Notify(1004);
                    throw new Exception();
                }

                Runtime.Log("City market is not full");

                string itemKey = ITEM_MAP + itemId;
                byte[] itemBytes = Storage.Get(Storage.CurrentContext, itemKey);

                if (itemBytes.Length <= 0)
                {
                    Runtime.Notify(1005);
                    throw new Exception();
                }

                Runtime.Log("Item on storage");

                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes);
                if (item.HERO <= 0)
                {
                    Runtime.Notify(1006);
                    throw new Exception();
                }

                BigInteger itemLordId = item.HERO;
                string heroKey = HERO_MAP + itemLordId.ToByteArray();
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                if (heroBytes.Length <= 0)
                {
                    Runtime.Notify(1007);
                    throw new Exception();
                }

                Runtime.Log("Item owner on storage");

                // Seller
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
                if (!Runtime.CheckWitness(hero.OWNER))
                {
                    Runtime.Notify(1008);
                    throw new Exception();
                }

                Runtime.Log("Item owner invoked marked adding method");

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

                Runtime.Log("Market item duration not expired");

                if (!AttachmentExistAB(durationFeeSettingBytes, GameOwner))
                {
                    Runtime.Notify(1010);
                    throw new Exception();
                }

                Runtime.Log("Fee attached");

                if (marketCofferAdditionSettingBytes.Length > 0)
                {
                    if (!marketCofferAdditionSettingBytes.Equals(cofferAdditionAmountBytes))
                    {
                        Runtime.Notify(1011, marketCofferAdditionSettingBytes, cofferAdditionAmountBytes);
                        throw new Exception();
                    }
                }

                MarketItemData marketItem = new MarketItemData();
                marketItem.Duration = duration;
                marketItem.Price = price;
                marketItem.City = cityId;
                marketItem.CreatedTime = Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp;
                marketItem.Seller = hero.OWNER;

                marketItemBytes = Neo.SmartContract.Framework.Helper.Serialize(marketItem);

                BigInteger cofferAdditionNum = (BigInteger)args[4];
                BigInteger cityCoffer = Helper.GetCoffer(cityId);
                cityCoffer = BigInteger.Add(cityCoffer, cofferAdditionNum);
                Helper.SetCoffer(cityId, cityCoffer);
                city.ItemsOnMarket = BigInteger.Add(city.ItemsOnMarket, 1);

                Runtime.Log("City data update");

                // Save on Storage!!!
                Storage.Put(Storage.CurrentContext, marketItemKey, marketItemBytes);

                //// Update City
                cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
                Storage.Put(Storage.CurrentContext, cityKey, cityBytes);

                Runtime.Notify(1000, itemId, price, duration, cityId);
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

                Runtime.Log("Item buying retreived");

                string itemKey = ITEM_MAP + itemId;
                byte[] itemBytes = Storage.Get(Storage.CurrentContext, itemKey);

                if (itemBytes.Length <= 0)
                {
                    Runtime.Notify(1005);
                    throw new Exception();
                }

                Runtime.Log("Item on blockchain");

                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes);
                if (item.HERO <= 0)
                {
                    Runtime.Notify(1006);
                    throw new Exception();
                }

                BigInteger itemLordId = item.HERO;
                string heroKey = HERO_MAP + itemLordId.ToByteArray();
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                if (heroBytes.Length <= 0)
                {
                    Runtime.Notify(1007);
                    throw new Exception();
                }

                //Runtime.Log("Item owned by someone");

                // Seller
                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
                if (Runtime.CheckWitness(hero.OWNER))
                {
                    Runtime.Notify(2004);
                    throw new Exception();
                }

                Runtime.Log("Buyer and seller are not the same");

                string buyerHeroKey = HERO_MAP + heroId.ToByteArray();
                byte[] buyerHeroBytes = Storage.Get(Storage.CurrentContext, buyerHeroKey);
                if (buyerHeroBytes.Length <= 0)
                {
                    Runtime.Notify(2005);
                    throw new Exception();
                }

                //Runtime.Log("Buyer on blockchain");

                Hero buyer = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(buyerHeroBytes);
                if (!Runtime.CheckWitness(buyer.OWNER))
                {
                    Runtime.Notify(2006);
                    throw new Exception();
                }

                Runtime.Log("Buyer owner called buying method");

                string marketItemKey = MARKET_MAP + itemId;
                byte[] marketItemBytes = Storage.Get(Storage.CurrentContext, marketItemKey);
                if (marketItemBytes.Length > 0)
                {
                    Runtime.Log("Market item on blockchain");

                    MarketItemData marketItem = (MarketItemData)Neo.SmartContract.Framework.Helper.Deserialize(marketItemBytes);
                    if (Blockchain.GetHeader(Blockchain.GetHeight()).Timestamp > marketItem.Duration + marketItem.CreatedTime)
                    {
                        Runtime.Notify(2007);
                        throw new Exception();
                    }
                    else
                    {
                        Runtime.Log("Market item duration not expired");
                        // original price based sum of money that buyer should attach to tx.
                        byte[] lordFeeSettingBytes = Storage.Get(Storage.CurrentContext, PERCENTS_LORD);
                        byte[] gameOwnerSettingBytes = Storage.Get(Storage.CurrentContext, PERCENTS_GAME_OWNER);

                        BigInteger marketCityId = marketItem.City;
                        string cityKey = CITY_MAP + marketCityId.ToByteArray();
                        City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, cityKey));

                        Runtime.Log("City returned");

                        BigInteger gameOwnerExpectation = 0;
                        BigInteger lordExpectation = 0;
                        BigInteger sellerExpectation = 0;

                        if (gameOwnerSettingBytes.Length > 0)
                        {
                            byte[] lordFeeBytes = (byte[])args[2];
                            if (!lordFeeBytes.Equals(lordFeeSettingBytes))
                            {
                                Runtime.Notify(2012, lordFeeSettingBytes, lordFeeBytes);
                                throw new Exception();
                            }

                            byte[] gameOwnerFeeBytes = (byte[])args[3];
                            if (!gameOwnerFeeBytes.Equals(gameOwnerSettingBytes))
                            {
                                Runtime.Notify(2013, gameOwnerSettingBytes, gameOwnerFeeBytes);
                                throw new Exception();
                            }

                            Runtime.Log("Pay correct fee");
                            BigInteger pricePercent = BigInteger.Divide(marketItem.Price, 100);

                            BigInteger lordFeePercents = (BigInteger)args[2];
                            BigInteger gameOwnerPercents = (BigInteger)args[3];

                            BigInteger lordFee = BigInteger.Multiply(pricePercent, lordFeePercents);
                            BigInteger gameOwnerFee = BigInteger.Multiply(pricePercent, gameOwnerPercents);

                            Runtime.Log("Lord percent fee");

                            // city is owned by buyer?
                            if (city.Hero > 0 && city.Hero == heroId)
                            {
                                sellerExpectation = marketItem.Price;
                                gameOwnerExpectation = gameOwnerFee;
                            }
                            // city owned by seller?
                            else if (city.Hero > 0 && city.Hero == item.HERO)
                            {
                                lordExpectation = BigInteger.Add(marketItem.Price, lordFee);
                                gameOwnerExpectation = gameOwnerFee;
                            }
                            // city is owned by NPC?
                            else if (city.Hero <= 0)
                            {
                                sellerExpectation = marketItem.Price;
                                gameOwnerExpectation = BigInteger.Add(gameOwnerFee, lordFee);
                            }
                            // city is owned by someone else?
                            else
                            {
                                sellerExpectation = marketItem.Price;
                                gameOwnerExpectation = gameOwnerFee;
                                lordExpectation = lordFee;
                            }

                            Runtime.Log("Lord expectation is increased");                            

                            Runtime.Log("Game owner expectation set");

                            byte[] sellerExpectationBytes = sellerExpectation.ToByteArray();
                            if (sellerExpectation > 0 && !AttachmentExistAB(sellerExpectationBytes, hero.OWNER))
                            {
                                Runtime.Notify(2008);
                                throw new Exception();
                            }

                            Runtime.Log("Check seller attahment fee");

                            if (city.Hero > 0)
                            {
                                BigInteger cityLordId = city.Hero;
                                string cityLordKey = HERO_MAP + cityLordId.ToByteArray();
                                Hero cityLord = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, cityLordKey));

                                byte[] lordExpectationBytes = lordExpectation.ToByteArray();
                                if (lordExpectation > 0 && !AttachmentExistAB(lordExpectationBytes, cityLord.OWNER))
                                {
                                    Runtime.Notify(2009, lordExpectationBytes, lordExpectation, lordFee, pricePercent, lordFeePercents);
                                    throw new Exception();
                                }
                            }

                            Runtime.Log("Check Lord attachment fee");

                            byte[] gameOwnerExpectationBytes = gameOwnerExpectation.ToByteArray();
                            if (gameOwnerExpectation > 0 && !AttachmentExistAB(gameOwnerExpectationBytes, GameOwner))
                            {
                                Runtime.Notify(2010, gameOwnerExpectationBytes, gameOwnerExpectation, gameOwnerFee, pricePercent, gameOwnerPercents);
                                throw new Exception();
                            }

                            Runtime.Log("Check game owner attachment fee");

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
                    Runtime.Notify(2014);
                    throw new Exception();
                }
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

                // Item can be removed from Blockchain before expiration, only by Item owner
                if (Blockchain.GetBlock(Blockchain.GetHeight()).Timestamp < mItem.Duration + mItem.CreatedTime)
                {
                    if (!Runtime.CheckWitness(mItem.Seller))
                    {
                        Runtime.Notify(3002);
                        throw new Exception();
                    }
                }

                BigInteger marketCityId = mItem.City;
                string cityKey = CITY_MAP + marketCityId.ToByteArray();
                byte[] cityBytes = Storage.Get(Storage.CurrentContext, cityKey);

                City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);
                city.ItemsOnMarket = BigInteger.Subtract(city.ItemsOnMarket, 1);

                Storage.Delete(Storage.CurrentContext, key);

                cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
                Storage.Put(Storage.CurrentContext, cityKey, cityBytes);

                Runtime.Notify(3000, itemId);
            }
            else if (param.Equals("logBattle"))
            {
                if (Runtime.CheckWitness(GeneralContract.GameOwner))
                {
                    Runtime.Notify(1);
                    throw new Exception();
                }

                // Prepare log
                BattleLog log = new BattleLog();

                log.BattleId = (byte[])args[0];
                log.BattleResult = (BigInteger)args[1]; // 0 - Attacker WON, 1 - Attacker Lose
                log.BattleType = (BigInteger)args[2];   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
                log.Attacker = (byte[])args[3]; // Hero
                log.AttackerTroops = (BigInteger)args[4];
                log.AttackerRemained = (BigInteger)args[5];
                log.DefenderObject = (byte[])args[6];   // City|Stronghold|Bandit Camp ID
                log.DefenderTroops = (BigInteger)args[7];
                log.DefenderRemained = (BigInteger)args[8];

                string battleIdKey = BATTLE_LOG_MAP + log.BattleId;
                byte[] battleLogBytes = Storage.Get(Storage.CurrentContext, battleIdKey);
                if (battleLogBytes.Length > 0)
                {
                    Runtime.Notify(7002);
                    throw new Exception();
                }

                // Get Hero
                string heroKey = HERO_MAP + log.Attacker;
                byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);
                if (heroBytes.Length <= 0)
                {
                    Runtime.Notify(7003);
                    throw new Exception();
                }

                Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
                if (!Runtime.CheckWitness(hero.OWNER))
                {
                    Runtime.Notify(7004);
                    throw new Exception();
                }

                log.AttackerItem1 = (byte[])args[9];
                log.AttackerItem2 = (byte[])args[10];
                log.AttackerItem3 = (byte[])args[11];
                log.AttackerItem4 = (byte[])args[12];
                log.AttackerItem5 = (byte[])args[13];

                Log.CheckItemOwnership(log.AttackerItem1, log.Attacker);
                Log.CheckItemOwnership(log.AttackerItem2, log.Attacker);
                Log.CheckItemOwnership(log.AttackerItem3, log.Attacker);
                Log.CheckItemOwnership(log.AttackerItem4, log.Attacker);
                Log.CheckItemOwnership(log.AttackerItem5, log.Attacker);

                BigInteger attackerNum = (BigInteger)args[3];
                if (attackerNum == 0)
                {
                    Runtime.Log("Hero Number is 0");
                }

                // Check Signature
                byte[] signature = (byte[])args[14];

                byte[] battleResultBytes = (byte[])args[1];
                byte[] battleTypeBytes = (byte[])args[2];
                byte[] signMessage = Neo.SmartContract.Framework.Helper.Concat(log.BattleId, battleResultBytes);

                Runtime.Log("Concatination of Defense was succ");

                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, battleTypeBytes);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, log.Attacker);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, log.DefenderObject);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, log.AttackerItem1);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, log.AttackerItem2);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, log.AttackerItem3);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, log.AttackerItem4);
                signMessage = Neo.SmartContract.Framework.Helper.Concat(signMessage, log.AttackerItem5);

                Runtime.Log("Concatted message data");

                Runtime.Log("Message converted");

                if (!VerifySignature(signMessage, signature, GameOwnerPublicKey))
                {
                    Runtime.Notify(2);
                    Runtime.Notify(signMessage);
                    Runtime.Notify(signature);
                    Runtime.Log("Signature Verification Failed");
                    throw new Exception();
                }
                BigInteger defenderObject = (BigInteger)args[6];

                Log.Battle(log, hero, attackerNum, args[15], defenderObject);
            }
            //else if (param.Equals("changeMarketFee"))
            //{
            //    // Available ranges are:
            //    BigInteger min = 0;
            //    BigInteger max = 70;

            //    // If city is owned by a player, then should be called by a player
            //    // If city is owned by NPC, then should be called by game owner
            //}
            //else if (param.Equals("changeCofferPercents"))
            //{
            //    // Should be called by game owner
            //    BigInteger min = 10;
            //    BigInteger max = 70;
            //}

            byte[] res = new byte[1] { 0 };
            return res;
        }

        /// <summary>
        /// Retrieve pseudo random number
        /// </summary>
        /// <param name="min">Min range</param>
        /// <param name="max">Max range</param>
        /// <returns>generated number</returns>
        public static BigInteger GetRandomNumber(BigInteger min, BigInteger max)
        {
            long numberOfTickets = (long)max;
            Header header = Blockchain.GetHeader(Blockchain.GetHeight());
            long randomNumber = (long)(header.ConsensusData >> 32);
            long winningTicket = (randomNumber * numberOfTickets) >> 32;
            //Runtime.Notify("The winning ticket is:", winningTicket);
            BigInteger ret = winningTicket;
            return ret;
        }

        public static bool AttachmentExistAB(byte[] value, byte[] receivingScriptHash)
        {
            if (value.Length <= 0)
                return true;
            Transaction TX = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = TX.GetOutputs();

            foreach (var output in outputs)
            {
                // Game Developers got their fee?
                if (output.ScriptHash.Equals(receivingScriptHash))
                {
                    Runtime.Log("The receiver has some money");
                    BigInteger val = output.Value;
                    Runtime.Notify("Values", output.Value, val, value);

                    if (val.ToByteArray().Equals(value))
                    {
                        return true;
                    }
                } else
                {
                    Runtime.Log("Not for receiver");
                    Runtime.Notify(output.Value, value, output.ScriptHash, receivingScriptHash);
                }
            }

            return false;
        }
    }
}
