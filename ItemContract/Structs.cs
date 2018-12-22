using System;
using System.Numerics;

namespace LordsContract
{

    /****************************************************************************
         * 
         * Data Structs for Game Data for Blockchain Storage.
         * 
         ****************************************************************************/
    [Serializable]
    public class MarketItemData
    {
        public BigInteger Price;                // Fixed Price of Item defined by Item owner
        public BigInteger AuctionDuration;      // 8, 12, 24 hours
        public BigInteger AuctionStartedTime;   // Unix timestamp in seconds
        public byte City;                       // City ID (item can be added onto the market only through cities.)
        public byte[] TX;                       // Transaction ID, (Transaction that has a record of Item Adding on Market).
        public byte[] Seller = new byte[33];    // Wallet Address of Item owner
    }

    [Serializable]
    public class Item
    {
        // STATIC DATA
        public byte STAT_TYPE;                  // Item can increase only one stat of Hero, there are five: Leadership, Defense, Speed, Strength and Intelligence
        public byte QUALITY;                    // Item can be in different Quality. Used in Gameplay.

        public BigInteger GENERATION;           // Items are given to Players only as a reward for holding Strongholds on map, or when players create a hero.
                                                // Items are given from a list of items batches. Item batches are putted on Blockchain at once by Game Owner.
                                                // Each of Item batches is called as a generation.

        // EDITABLE DATA
        public BigInteger STAT_VALUE;
        public BigInteger LEVEL;
        public BigInteger XP;                   // Each battle where, Item was used by Hero, increases Experience (XP). Experiences increases Level. Level increases Stat value of Item
        public byte[] OWNER;                    // Wallet address of Item owner.
    }

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
        public BigInteger TROOPS_CAP;           // Troops limit for this hero
        public BigInteger LEADERSHIP;           // Leadership Stat value
        public BigInteger INTELLIGENCE;         // Intelligence Stat value
        public BigInteger STRENGTH;             // Strength Stat value
        public BigInteger SPEED;                // Speed Stat value
        public BigInteger DEFENSE;              // Defense Stat value
        public byte[] TX;                       // Transaction ID where Hero creation was recorded
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
        public BigInteger CreatedBlock;         // The Blockchain Height
        public BigInteger Coffer;               // City Coffer
        public BigInteger Size;
    }

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

}


