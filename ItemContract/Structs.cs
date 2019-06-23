using System;
using System.Numerics;

namespace LordsContract
{

    /****************************************************************************
     * 
     * Structs to managable data of Game
     * 
     ****************************************************************************/
    [Serializable]
    public class MarketItemData
    {
        public BigInteger Price;                // Price of Item defined by Item owner
        public BigInteger Duration;      // 8, 12, 24 hours
        public BigInteger CreatedTime;   // Unix timestamp in seconds
        public BigInteger City;                 // City ID (item can be added onto the market only through cities.)
        //public byte[] TX;                       // Transaction ID, (Transaction that has a record of Item Adding on Market).
        public byte[] Seller = new byte[20];    // Wallet Address of Item owner

        //public byte[] Lengths;  // Price, 1, ??, 1, 33, 20
    }

    [Serializable]
    public class Item
    {
        // Static data. Once they are inputted, they will not be edited again!
        public BigInteger STAT_TYPE;            // There are five stat types: Leadership, Defense, Speed, Strength and Intelligence
        public BigInteger QUALITY;              // Just a quality parameter
        public BigInteger GENERATION;           // Belonged batch ID

        // EDITABLE DATA
        public BigInteger STAT_VALUE;
        public BigInteger LEVEL;
        public BigInteger XP;                   // Each battle where, Item was used by Hero, increases Experience (XP). Experiences increases Level. Level increases Stat value of Item
        public BigInteger HERO;                    // Wallet address of Item owner.

        public BigInteger BATCH;                // Batch type of item. Either Stronghold Reward, or hero Creation
    }

    // Serialize manually, since it is used for out-of-blockchain use with a getStorage method
    [Serializable]
    public class DropData                       // Information of Item that player can get as a reward.
    {
        public BigInteger Block;                // Blockchain Height, in which player got Item as a reward
        public BigInteger StrongholdId;         // Stronghold on the map, for which player got Item
        public byte[] ItemId;               // Item id that was given as a reward
        public BigInteger HeroId;
    }

    [Serializable]
    public class Hero
    {
        public byte[] OWNER;                    // Wallet address of Player that owns Hero
        //public BigInteger TROOPS_CAP;           // Troops limit for this hero
        public byte[] LEADERSHIP;           // Leadership Stat value
        public byte[] INTELLIGENCE;         // Intelligence Stat value
        public byte[] STRENGTH;             // Strength Stat value
        public byte[] SPEED;                // Speed Stat value
        public byte[] DEFENSE;              // Defense Stat value
        public BigInteger ID;
        public BigInteger StrongholdsAmount;
    }

    [Serializable]
    public class Stronghold
    {
        public BigInteger ID;                   // Stronghold ID
        public BigInteger Hero;                 // Hero ID, that occupies Stronghold on map
        public BigInteger CreatedBlock;         // The Blockchain Height
    }

    [Serializable]
    public class City
    {
        public BigInteger ID;                   // Stronghold ID
        public BigInteger Hero;                 // Hero ID, that occupies City on map
        public BigInteger Size;
        public BigInteger ItemsOnMarket;        // Current amount of items on city market
        public BigInteger ItemsCap;
        public BigInteger CofferBlock;
        public BigInteger CofferPayoutPercents;
        public BigInteger MarketFee;
    }

    // Serialize manually, since it is used for out-of-blockchain use with a getStorage method
    [Serializable]
    public class BattleLog
    {
        public byte[] BattleId;
        public BigInteger BattleResult; // 0 - Attacker WON, 1 - Attacker Lose
        public BigInteger BattleType;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
        public byte[] Attacker;
        public byte[] AttackerOwner;
        public BigInteger AttackerTroops;       // Attacker's troops amount that were involved in the battle
        public BigInteger AttackerRemained;     // Attacker's remained troops amount
        public byte[] AttackerItem1;        // Item IDs that were equipped by Attacker during battle.
        public byte[] AttackerItem2;
        public byte[] AttackerItem3;
        public byte[] AttackerItem4;
        public byte[] AttackerItem5;
        public byte[] DefenderObject;   // City|Stronghold|NPC ID based on battle type
        public BigInteger DefenderTroops;
        public BigInteger DefenderRemained; // Remained amount of troops
        public BigInteger Time;             // Unix Timestamp in seconds. Time, when battle happened 
    }

    //[Serializable]
    public class CofferPayment
    {
        public BigInteger LastPaidCity;
        public BigInteger Block;                   // Session Start
        public BigInteger BlockEnd;
    }
}


