using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System.Numerics;

namespace LordsContract
{
    public class GeneralContract : SmartContract
    {
        public static readonly string
            MARKET_PREFIX = "\x01\x00",
            MANAGABLE_ITEM_PREFIX = "\x02\x00",
            STRONGHOLD_PREFIX = "\x03\x00",
            STRONGHOLD_REWARD_PREFIX = "\x04\x00",
            BATTLE_LOG_PREFIX = "\x05\x00",
            NEXT_REWARD_ITEM_KEY = "\x06",
            CITY_PREFIX = "\x07\x00",
            HERO_PREFIX = "\x08\x00",
            LATEST_REWARDED_ITEM_KEY = "\x09",
            COFFER_PREFIX = "\x10\x00",
            COFFER_PAYOUT_KEY = "\x11",
            UPDATED_STAT_PREFIX = "\x12\x00",
            TEST_KEY = "\x13",
            VERIFICATIONAL_ITEM_PREFIX = "\x14\x00",
            DROPPED_INCREMENTOR = "\x15";

        public static readonly BigInteger CityType = 0, StrongholdType = 1, BanditCampType = 2;

        // Items may be given to heroes in two situation: when they create hero or when they own some territory on the game map.
        public static readonly byte HERO_CREATION_BATCH = 0;
        public static readonly byte STRONGHOLD_REWARD_BATCH = 1;
        public static readonly byte NO_BATCH = 2;

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
                                       auction12HoursFee = 20_000_000,          // 0.2 GAS
                                       auction24HoursFee = 30_000_000,          // 0.3 GAS
                                       heroCreationFee = 500_000_000,           // 5.0 GAS
                                       cityAttackFee = 200_000_000,             // 2.0 GAS
            strongholdAttackFee = 100_000_000,                                       // 1.0 GAS
            banditCampAttackFee = 50_000_000,                                       // 0.5 GAS
            bigCityCoffer = 100_000_000,                                      // 1.0 GAS
            mediumCityCoffer = 88_000_000,                                       // 0.8 GAS
            smallCityCoffer = 50_000_000                                       // 0.5 GAS
            ;

        // Item can be on Market for 8, 12, 24 hours. If someone tries to buy item on market after expiration,
        // Then, buying item will be invalid.
        public static readonly BigInteger duration8Hours = 28800;                  // 28_800 Seconds are 8 hours
        public static readonly BigInteger duration12Hours = 43200;                 // 43_200 Seconds are 12 hours
        public static readonly BigInteger duration24Hours = 86400;                 // 86_400 Seconds are 24 hours

        // The Smartcontract Owner's Wallet Address. Used to receive some Gas as a transaction fee.
        public static readonly byte[] GameOwner = "AK2nJJpJr6o664CWJKi1QRXjqeic2zRp8y".ToScriptHash();//"AML8hyTV4vXuomovxdcAH9pRC9ny618YmA".ToScriptHash();
       
        public static readonly BigInteger CofferPayoutInterval = 25000;            // In Blocks. Each blocks generated in 20-30 seconds.

        public static readonly BigInteger bigCity = 1, mediumCity = 2, smallCity = 3;

        public static readonly BigInteger bigMarketCap = 20, mediumMarketCap = 15, smallMarketCap = 10;
        public static readonly BigInteger bigTroopsCap = 500, mediumTroopsCap = 400, smallTroopsCap = 300;
        public static readonly BigInteger bigDefense = 50_000_000, mediumDefense = 55_000_000, smallDefense = 60_000_000;
                
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
                return Periodical.CofferPayout();
            }
            else if (param.Equals("dropItems"))
            {
                return Periodical.SimpleDropItems((BigInteger)args[0]);
            }

            else if (param.Equals("putCity"))
            {
                return Put.City((BigInteger)args[0], (BigInteger)args[1]);
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
                item.LEVEL = (BigInteger)args[6];           // ???
                item.OWNER = new byte[] { };                // 20
                item.XP = 0;                                // ???
                //item.INITIAL = (BigInteger)args[7];         // 1
                item.OWNER = ExecutionEngine.CallingScriptHash;
                item.BATCH = (byte)args[0];

                Put.Item((BigInteger)args[1], item);
            }
            else if (param.Equals("putHero"))
            {
                if (args.Length != 11)
                {
                    Runtime.Log("Invalid amount parameters");
                    return new BigInteger(0).AsByteArray();
                }

                Runtime.Log("Hero putting initialized");
                Hero hero = new Hero();
                hero.OWNER = ExecutionEngine.CallingScriptHash;
                //hero.TROOPS_CAP = (BigInteger)args[2];
                hero.INTELLIGENCE = (BigInteger)args[1];
                hero.SPEED = (BigInteger)args[2];
                hero.STRENGTH = (BigInteger)args[3];
                hero.LEADERSHIP = (BigInteger)args[4];
                hero.DEFENSE = (BigInteger)args[5];
                //hero.TX = ((Transaction)ExecutionEngine.ScriptContainer).Hash;

                hero.Equipments = new BigInteger[5] { (BigInteger)args[6], (BigInteger)args[7], (BigInteger)args[8], (BigInteger)args[9], (BigInteger)args[10] };
                hero.EquipmentsAmount = 5;

                return Put.Hero((BigInteger)args[0], hero);
            }
            else if (param.Equals("putStronghold"))
            {
                string key;
                Stronghold stronghold = new Stronghold();
                byte[] bytes;

                stronghold.Hero = (BigInteger)args[1];
                stronghold.ID = (BigInteger)args[0];
                stronghold.CreatedBlock = Blockchain.GetHeight();
            
                key = GeneralContract.STRONGHOLD_PREFIX + stronghold.ID.AsByteArray();
                bytes = Neo.SmartContract.Framework.Helper.Serialize(stronghold);

                Storage.Put(Storage.CurrentContext, key, bytes);

                Runtime.Log("Stronghold data has been inserted");

                return new BigInteger(0).AsByteArray();
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
            //else if (param.Equals("logStrongholdLeave"))
            //{
            //    return Log.StrongholdLeave(args);
            //}
            //else if (param.Equals("changeTroopsAmount"))
            //{
            //    return ChangeTroopsAmount((BigInteger)args[0], (BigInteger)args[1]);
            //}
            //else if (param.Equals("changeEquipments"))
            //{
            //    return ChangeEquipments((BigInteger)args[0], (BigInteger[])args[1]);
            //}       
            //else if (param.Equals("setForTestStatus"))
            //{
            //    return SetForTestStatus((bool)args[0]);
            //} 
            

            //Runtime.Notify("Incorrect Parameter");
            return new BigInteger(1).AsByteArray();
        }

        //public static byte[] ChangeTroopsAmount(BigInteger cityId, BigInteger additionalTroops)
        //{
        //    // Get City information
        //    // is owner of city called this method
        //    // is amount of troops are not going over limit
        //    // then, save information
        //    string cityKey = GeneralContract.CITY_PREFIX + cityId;

        //    // Check existence of City on Blockchain
        //    byte[] cityBytes = Storage.Get(Storage.CurrentContext, cityKey);
        //    if (cityBytes.Length == 0)
        //    {
        //        Runtime.Notify("City doesn't exist on Blockchain!");
        //        return new BigInteger(0).AsByteArray();
        //    }

        //    // Check, that method invoker is lord of city
        //    City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);
        //    if (city.Hero == 0)
        //    {
        //        Runtime.Notify("City has no owner"); 
        //        return new BigInteger(0).AsByteArray();
        //    }

        //    // City holds ID of Owner, so get the Hero data by ID, and see the Wallet address of player
        //    string heroKey = GeneralContract.HERO_PREFIX + city.Hero;
        //    Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, heroKey));

        //    if (!Runtime.CheckWitness(hero.OWNER))
        //    {
        //        Runtime.Notify("Only Lord of city able to change amount of troops in a city!");
        //        return new BigInteger(0).AsByteArray();
        //    }

        //    // Check that troops cap is not reached
        //    city.Troops = city.Troops + additionalTroops;

        //    if (city.Troops > GetTroopsCap(city.Size))
        //    {
        //        Runtime.Notify("Too many troops for defend.");
        //        return new BigInteger(0).AsByteArray();
        //    }

        //    cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
        //    Storage.Put(Storage.CurrentContext, cityKey, cityBytes);

        //    return new BigInteger(1).AsByteArray();
        //}

        //// Equipments order matter. Index at argument is for item type of equipment
        //public static byte[] ChangeEquipments(BigInteger heroId, BigInteger[] equipments)
        //{
        //    // Does Hero belong to Smartcontract owner
        //    string heroKey = HERO_PREFIX + heroId.AsByteArray();
        //    byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);

        //    if (heroBytes.Length == 0)
        //    {
        //        Runtime.Notify("Invalid Hero ID");
        //        return new BigInteger(0).AsByteArray();
        //    }

        //    Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
        //    if (!Runtime.CheckWitness(hero.OWNER))
        //    {
        //        Runtime.Notify("Only Hero owner may change equipment set");
        //        return new BigInteger(0).AsByteArray();
        //    }

        //    // Is Equipments amount in limit range
        //    if (equipments.Length != 5)
        //    {
        //        Runtime.Notify("Hero may have 5 equipments at once");
        //        return new BigInteger(0).AsByteArray();
        //    }

        //    BigInteger statType = 1;
        //    for (var i = 0; i < 5; i++)
        //    {
        //        if (equipments[i] == 0)
        //        {
        //            statType = statType + 1;
        //            continue;
        //        }
        //        string itemKey = MANAGABLE_ITEM_PREFIX + equipments[i].AsByteArray();
        //        byte[] itemBytes = Storage.Get(Storage.CurrentContext, itemKey);

        //        if (itemBytes.Length == 0)
        //        {
        //            Runtime.Notify("Invalid Item ID");
        //            return new BigInteger(0).AsByteArray();
        //        }

        //        Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes);

        //        // Does item belong to Hero Owner
        //        if (!item.OWNER.Equals(hero.OWNER))
        //        {
        //            Runtime.Notify("Only Item owner may do operation with an item");
        //            return new BigInteger(0).AsByteArray();
        //        }

        //        // Does Equipment have a right stat type
        //        if (item.STAT_TYPE != statType)
        //        {
        //            Runtime.Notify("Invalid Stat type", statType);
        //            return new BigInteger(0).AsByteArray();

        //        }
        //        statType = statType + 1;
        //    }

        //    // Update Current Equipments
        //    hero.Equipments = equipments;
        //    hero.EquipmentsAmount = equipments.Length;

        //    heroBytes = Neo.SmartContract.Framework.Helper.Serialize(hero);
        //    Storage.Put(Storage.CurrentContext, heroKey, heroBytes);

        //    return new BigInteger(1).AsByteArray();
        //}

        //public static bool IsForTest()
        //{
        //    byte[] res = Storage.Get(Storage.CurrentContext, TEST_KEY);
        //    // By Default, Test Purpose is True
        //    if (res.Length == 0 )
        //    {
        //        return true;
        //    }
        //    BigInteger big = res.AsBigInteger();
        //    if (big.IsZero)
        //    {
        //        return false;
        //    }
        //    return true;
        //}

        //public static byte[] SetForTestStatus(bool status)
        //{
        //    if (!Runtime.CheckWitness(GameOwner))
        //    {
        //        Runtime.Notify("Only Game Owner may set the flag of Testing");
        //        return new BigInteger(0).AsByteArray();
        //    }

        //    byte[] value = new BigInteger(0).AsByteArray();
        //    if (status)
        //        value = new BigInteger(1).AsByteArray();
        //    Storage.Put(Storage.CurrentContext, TEST_KEY, value);

        //    return new BigInteger(1).AsByteArray();
        //}

        //------------------------------------------------------------------------------------
        //
        // Helpers used in Smartcontract
        //
        //------------------------------------------------------------------------------------
        public static BigInteger GetRandomNumber(ulong max = 10)
        {
            Header header = Blockchain.GetHeader(Blockchain.GetHeight());
            ulong randomNumber = header.ConsensusData;
            ulong percentage = (randomNumber % max);

            BigInteger bigRandom = percentage;

            return bigRandom;
        }

        //public static BigInteger GetTroopsCap(BigInteger citySize)
        //{
        //    if (citySize == bigCity)
        //    {
        //        return bigTroopsCap;
        //    }
        //    else if (citySize == mediumCity)
        //    {
        //        return mediumTroopsCap;
        //    }
        //    return smallTroopsCap;
        //}

        public static bool IsTransactionOutputExist(decimal value)
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
    }
}
