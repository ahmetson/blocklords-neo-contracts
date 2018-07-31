using BattleContract.Character;
using BattleContract.GameComponents;
using BattleContract.Math;
using BattleContract.StorageData;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace BattleContract.Battle
{
    public class Helper
    {
        public static BigInteger XP_1 = 500;
        public static BigInteger XP_2 = 100;


        public static int GetBattleType(bool isMyNpc, bool isEnemyNpc)
        {
            if (isMyNpc) return 2;              // 2 = PVE
            if (isEnemyNpc) return 2;
            return 1;                           // 1 = PVP
        }

        public static TotalRate Create(ItemIncreasing[] Increasings, Range idsRange)
        {
            TotalRate totalRate = new TotalRate(idsRange);

            for (int i = 0, rangeMin = 1, idsNum = Math.Helper.GetLength(idsRange); i < idsNum; i++)
            {
                TotalRate.SetRateByIndex(totalRate, i, new Range(rangeMin, Increasings[totalRate.Ids[i]].Rate));
                rangeMin = Increasings[totalRate.Ids[i]].Rate + 1;    // The Min of Next Rate is the next number after the current rates Max
                
                totalRate.Total = totalRate.Total + TotalRate.GetRateByIndex(totalRate, i).Max;
            }

            return totalRate;
        }

        public static int GetIdByRate(TotalRate totalRate, int rate)
        {
            for (int i = 0; i < 5; i++)
            {
                Range range = TotalRate.GetRateByIndex(totalRate, i);
                if (Math.Helper.InRange(range, rate))
                {
                    return totalRate.Ids[i];
                }
            }

            return 0;
        }
        

        public static BigInteger DamageCalculation(BigInteger myStrength, BigInteger mySpeed, BigInteger enemyDefense, int advantage, int cityDefence = 0)
        {
            Runtime.Log("Damage calculation");
            if (mySpeed == 0) Runtime.Log("Speed is zero");
            if (mySpeed > 0) Runtime.Log("Speed is greater than 0");
            if (mySpeed < 0) Runtime.Log("Speed is less than 0");
            BigInteger speed = (mySpeed / (mySpeed + XP_2));
            Runtime.Log("Speed prepared");
            BigInteger defensePart1 = 1 - enemyDefense;
            BigInteger defensePart2 = XP_1 + enemyDefense;
            Runtime.Log("Defense parts are calculation");
            BigInteger defense = defensePart1 / defensePart2;
            Runtime.Log("Defense prepared");
            BigInteger cityDefense = 1 - cityDefence;
            Runtime.Log("City defense prepared");
            BigInteger damage = myStrength * defense * speed * advantage * cityDefense;
            Runtime.Log("Damage prepared");
            return damage;
        }

        public static BigInteger AcceptDamage(BigInteger damage, int troops)
        {
            //BigInteger troopsBig = new BigInteger(2);
            BigInteger remainedTroops = troops - damage;
            if (remainedTroops < 0) return 0;
    
            return remainedTroops;
        }
        /**
         * Return Result:
         * 0 - no one win or lose
         * 1 - win first hero
         * 2 - win second hero
         * 3 - both of the heroes lose
         */
        public static BigInteger CalculateBattleResult(BigInteger myTroops, BigInteger myRemained, BigInteger enemyTroops, BigInteger enemyRemained)
        {
            BigInteger myPercent = myTroops / 100;
            BigInteger myRemainedPercents = myRemained / myPercent;

            BigInteger enemyPercent = myTroops / 100;
            BigInteger enemyRemainedPercents = enemyRemained / enemyPercent;

            if (enemyRemainedPercents < 30)
            {
                if (myRemainedPercents >= 30)
                {
                    return 0;// BattleResult.MY_WIN;
                }
                else
                {
                    return 2;// BattleResult.BOTH_LOSE;
                }
            }
            if (myRemainedPercents < 30)
            {
                if (enemyRemainedPercents >= 30)
                {
                    return 1;// BattleResult.ENEMY_WIN;
                }
                else
                {
                    return 2;// BattleResult.BOTH_LOSE;
                }
            }

            if (myRemainedPercents.Equals(enemyRemainedPercents))
            {
                return 2;// BattleResult.BOTH_LOSE;
            }

            if (myRemainedPercents > enemyRemainedPercents)
            {
                return 0;// BattleResult.MY_WIN;
            }
            // else
            return 1;// BattleResult.ENEMY_WIN;
        }

        public static int GetIncreaseValue(ItemIncreasing[] increasingItems, Item item, int battleType)
        {
            Range ids = GetBattleTypeRange(battleType);         // Range Of IDS For Battle Type
            ids = GetQualityTypeRange(ids, item.Quality);       // Inner Range of IDS For Item Quality Type
            ids = GetStatTypeRange(ids, item.statType);         // Inner Range of IDS For Item's Stat Type
            TotalRate rates = Create(increasingItems, ids);       // Build the total weight of all IDs

            int randomRate = Math.Helper.GetRandomNumber(rates.Total); // Call GetRandomNumber(rates.total);
            int id = GetIdByRate(rates, randomRate);              // Returns assigned ID for rate
            return increasingItems[id].Increasing;
        }
        public static void IncreaseStat(Item item, int increaseValue, byte[] owner)
        {
            BigInteger stat = item.Stat.AsBigInteger() + increaseValue;
            BigInteger maxStat = item.MaxStat.AsBigInteger();

            if (stat > maxStat)
            {
                Runtime.Log("over_max_stat_value");
                if (!item.Stat.Equals(item.MaxStat))
                    return;
                Runtime.Log("Set to Maximum");
                item.Stat = item.MaxStat;
            }
            else
            {
                item.Stat = stat.AsByteArray();
            }

            //ItemDataHelper.PutItem(item, owner);
        }

        private static Range GetBattleTypeRange(int battleType)
        {
            if (battleType==1)
            {
                return new Range(1, 75);
            }
            return new Range(76, 150);
        }
        private static Range GetQualityTypeRange(Range range, QualityType qualityType)
        {
            int qualityRangeDistance = 25;
            int quality = 0;

            if (qualityType == QualityType.SSR)
            {
                quality = 0;
            }
            else if (qualityType == QualityType.SR)
            {
                quality = 1;
            }
            else
            {
                quality = 2;
            }

            Range newRange = range;
            newRange.Min = range.Min + (quality * qualityRangeDistance);
            newRange.Max = newRange.Min + qualityRangeDistance - 1;
            return newRange;
        }
        private static Range GetStatTypeRange(Range range, StatType statType)
        {
            int statRangeDistance = 5;
            int stat = 0;

            if (statType==StatType.Leadership)
            {
                stat = 0;
            }
            else if (statType==StatType.Defense)
            {
                stat = 1;
            }
            else if (statType==StatType.Speed)
            {
                stat = 2;
            }
            else if (statType==StatType.Strength)
            {
                stat = 3;
            }
            else
            {
                stat = 4;
            }
            Range newRange = range;
            newRange.Min = range.Min + (stat * statRangeDistance);
            newRange.Max = newRange.Min + statRangeDistance - 1;        // -1 at the end is added by newRange.Min itself. Which is always equal to 1
            return newRange;
        }

    }
}
