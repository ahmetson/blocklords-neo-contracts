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

        public static readonly BigInteger CityType = 0, StrongholdType = 1, BanditCampType = 2;

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
                item.OWNER = new byte[] { };
                item.XP = 0;
                item.INITIAL = (bool)args[7];

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

                hero.Equipments = new BigInteger[5] { (BigInteger)args[9], (BigInteger)args[10], (BigInteger)args[11], (BigInteger)args[12], (BigInteger)args[13] };
                hero.EquipmentsAmount = 5;

                return Put.Hero((BigInteger)args[0], hero, (byte[])args[8]);
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
            else if (param.Equals("logBattle"))
            {
                return Log.Battle(args);
            }
            else if (param.Equals("logStrongholdLeave"))
            {
                return Log.StrongholdLeave(args);
            }
            else if (param.Equals("lhangeTroopsAmount"))
            {
                return ChangeTroopsAmount((BigInteger)args[0], (BigInteger)args[1]);
            }
            else if (param.Equals("changeEquipments"))
            {
                return ChangeEquipments((BigInteger)args[0], (BigInteger[])args[1]);
            }

            //Runtime.Notify("Incorrect Parameter");
            return new BigInteger(1).AsByteArray();
        }

        public static byte[] ChangeTroopsAmount(BigInteger cityId, BigInteger additionalTroops)
        {
            // Get City information
            // is owner of city called this method
            // is amount of troops are not going over limit
            // then, save information
            string cityKey = GeneralContract.CITY_PREFIX + cityId;

            // Check existence of City on Blockchain
            byte[] cityBytes = Storage.Get(Storage.CurrentContext, cityKey);
            if (cityBytes.Length == 0)
            {
                Runtime.Notify("City doesn't exist on Blockchain!");
                return new BigInteger(0).AsByteArray();
            }

            // Check, that method invoker is lord of city
            City city = (City)Neo.SmartContract.Framework.Helper.Deserialize(cityBytes);
            if (city.Hero == 0)
            {
                Runtime.Notify("City has no owner"); 
                return new BigInteger(0).AsByteArray();
            }

            // City holds ID of Owner, so get the Hero data by ID, and see the Wallet address of player
            string heroKey = GeneralContract.HERO_PREFIX + city.Hero;
            Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(Storage.Get(Storage.CurrentContext, heroKey));

            if (!Runtime.CheckWitness(hero.OWNER))
            {
                Runtime.Notify("Only Lord of city able to change amount of troops in a city!");
                return new BigInteger(0).AsByteArray();
            }

            // Check that troops cap is not reached
            city.Troops = city.Troops + additionalTroops;

            if (city.Troops > GetTroopsCap(city.Size))
            {
                Runtime.Notify("Too many troops for defend.");
                return new BigInteger(0).AsByteArray();
            }

            cityBytes = Neo.SmartContract.Framework.Helper.Serialize(city);
            Storage.Put(Storage.CurrentContext, cityKey, cityBytes);

            return new BigInteger(1).AsByteArray();
        }

        public static byte[] ChangeEquipments(BigInteger heroId, BigInteger[] equipments)
        {
            // Does Hero belong to Smartcontract owner
            string heroKey = HERO_PREFIX + heroId.AsByteArray();
            byte[] heroBytes = Storage.Get(Storage.CurrentContext, heroKey);

            if (heroBytes.Length == 0)
            {
                Runtime.Notify("Invalid Hero ID");
                return new BigInteger(0).AsByteArray();
            }

            Hero hero = (Hero)Neo.SmartContract.Framework.Helper.Deserialize(heroBytes);
            if (!Runtime.CheckWitness(hero.OWNER))
            {
                Runtime.Notify("Only Hero owner may change equipment set");
                return new BigInteger(0).AsByteArray();
            }

            // Is Equipments amount in limit range
            if (equipments.Length > 5)
            {
                Runtime.Notify("Hero has maximum 5 equipments at once");
                return new BigInteger(0).AsByteArray();
            }

            bool type1 = false, type2 = false, type3 = false, type4 = false, type5 = false;
            for (var i = 0; i < equipments.Length; i++)
            {
                string itemKey = ITEM_PREFIX + equipments[i].AsByteArray();
                byte[] itemBytes = Storage.Get(Storage.CurrentContext, itemKey);

                if (itemBytes.Length == 0)
                {
                    Runtime.Notify("Invalid Item ID");
                    return new BigInteger(0).AsByteArray();
                }

                Item item = (Item)Neo.SmartContract.Framework.Helper.Deserialize(itemBytes);

                // Do equipments belong to Hero Owner
                if (!item.OWNER.Equals(hero.OWNER))
                {
                    Runtime.Notify("Only Item owner may do operation with an item");
                    return new BigInteger(0).AsByteArray();
                }

                // Do they are for different types
                if (item.STAT_TYPE == 1)
                {
                    if (type1)
                    {
                        Runtime.Notify("Each stat type must have one equipment on hero");
                        return new BigInteger(0).AsByteArray();
                    }
                    else
                    {
                        type1 = true;
                    }
                }
                else if (item.STAT_TYPE == 3)
                {
                    if (type3)
                    {
                        Runtime.Notify("Each stat type must have one equipment on hero");
                        return new BigInteger(0).AsByteArray();
                    }
                    else
                    {
                        type3 = true;
                    }
                }
                else if (item.STAT_TYPE == 2)
                {
                    if (type2)
                    {
                        Runtime.Notify("Each stat type must have one equipment on hero");
                        return new BigInteger(0).AsByteArray();
                    }
                    else
                    {
                        type2 = true;
                    }
                }
                else if (item.STAT_TYPE == 4)
                {
                    if (type4)
                    {
                        Runtime.Notify("Each stat type must have one equipment on hero");
                        return new BigInteger(0).AsByteArray();
                    }
                    else
                    {
                        type4 = true;
                    }
                }
                else if (item.STAT_TYPE == 5)
                {
                    if (type5)
                    {
                        Runtime.Notify("Each stat type must have one equipment on hero");
                        return new BigInteger(0).AsByteArray();
                    }
                    else
                    {
                        type5 = true;
                    }
                }
            }

            // Update Current Equipments
            hero.Equipments = equipments;
            hero.EquipmentsAmount = equipments.Length;

            heroBytes = Neo.SmartContract.Framework.Helper.Serialize(hero);
            Storage.Put(Storage.CurrentContext, heroKey, heroBytes);

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

        public static BigInteger GetTroopsCap(BigInteger citySize)
        {
            if (citySize == bigCity)
            {
                return bigTroopsCap;
            }
            else if (citySize == mediumCity)
            {
                return mediumTroopsCap;
            }
            return smallTroopsCap;
        }
    }
}
