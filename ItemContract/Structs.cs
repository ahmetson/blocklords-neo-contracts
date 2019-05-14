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
        public BigInteger ItemId;               // Item id that was given as a reward
        public BigInteger HeroId;
    }

    [Serializable]
    public class Hero
    {
        public byte[] OWNER;                    // Wallet address of Player that owns Hero
        //public BigInteger TROOPS_CAP;           // Troops limit for this hero
        public BigInteger LEADERSHIP;           // Leadership Stat value
        public BigInteger INTELLIGENCE;         // Intelligence Stat value
        public BigInteger STRENGTH;             // Strength Stat value
        public BigInteger SPEED;                // Speed Stat value
        public BigInteger DEFENSE;              // Defense Stat value
        //public byte[] TX;                       // Transaction ID where Hero creation was recorded
        public BigInteger[] Equipments;         // Items that are equipped on hero
        public BigInteger EquipmentsAmount;     // Amount of items that are equipped on hero
        //public BigInteger Banned;             // Whether hero is banned by Game Admin for Cheating or not
        public BigInteger ID;
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
        public decimal Coffer;               // City Coffer
        public BigInteger Size;
        public BigInteger ItemsOnMarket;        // Current amount of items on city market
        public BigInteger ItemsCap;
    }

    // Serialize manually, since it is used for out-of-blockchain use with a getStorage method
    [Serializable]
    public class BattleLog
    {
        public BigInteger BattleId;
        public BigInteger BattleResult; // 0 - Attacker WON, 1 - Attacker Lose
        public BigInteger BattleType;   // 0 - City, 1 - Stronghold, 2 - Bandit Camp
        public BigInteger Attacker;
        public byte[] AttackerOwner;
        public BigInteger AttackerTroops;       // Attacker's troops amount that were involved in the battle
        public BigInteger AttackerRemained;     // Attacker's remained troops amount
        public BigInteger AttackerItem1;        // Item IDs that were equipped by Attacker during battle.
        public BigInteger AttackerItem2;
        public BigInteger AttackerItem3;
        public BigInteger AttackerItem4;
        public BigInteger AttackerItem5;
        public BigInteger DefenderObject;   // City|Stronghold|NPC ID based on battle type

        public BigInteger Defender;         // City Owner ID|Stronghold Owner ID or NPC ID
        public byte[] DefenderOwner;
        public BigInteger DefenderTroops;
        public BigInteger DefenderRemained; // Remained amount of troops
        public BigInteger DefenderItem1;
        public BigInteger DefenderItem2;
        public BigInteger DefenderItem3;
        public BigInteger DefenderItem4;
        public BigInteger DefenderItem5;

        public BigInteger Time;             // Unix Timestamp in seconds. Time, when battle happened 
        public byte[] TX;                   // Transaction where Battle Log was recorded.
    }

    // Serialize manually, since it is used for out-of-blockchain use with a getStorage method
    [Serializable]
    public class UpdatedItem
    {
        public BigInteger ItemId;                   // Item ID
        public BigInteger IncreaseValue;
    }
}


