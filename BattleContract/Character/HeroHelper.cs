using BattleContract.GameComponents;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace BattleContract.Character
{
    public class Helper
    {

        public static int GetAdvantage(int my, int enemy)
        {
            Runtime.Notify("Count the advantage", my, enemy);
            
            // Rider = 0, Archer = 1, Soldier = 2
            if (my == 0)
            {
                if (enemy==1) return 2;
            }
            if (my==1)
            {
                if (enemy==2) return 2;
            }
            if (my==2)
            {
                if (enemy==0) return 2;
            }
            return 1;
        }
        public static int StringToClassType(string value)
        {
            int classType = 0;

            if (value.Equals("0"))
            {
                classType = 0;// ClassType.Rider;
            }
            if (value.Equals("1"))
            {
                classType = 1;// ClassType.Archer;
            }
            if (value.Equals("2"))
            {
                classType = 2;// ClassType.Soldier;
            }
            
            return classType;
        }
        public static string ClassTypeToString(int classType)
        {
            string value = "0";

            if (classType==0)
            {
                value = "0";
            }
            if (classType==1)
            {
                value = "1";
            }
            if (classType==2)
            {
                value = "2";
            }
           
            return value;
        }

        public static StatType StringToStatType(string value)
        {
            StatType statType = StatType.Defense;

            if (value.Equals("0"))
            {
                statType = StatType.Leadership;
            }
            if (value.Equals("1"))
            {
                statType = StatType.Defense;
            }
            if (value.Equals("2"))
            {
                statType = StatType.Speed;
            }
            if (value.Equals("4"))
            {
                statType = StatType.Strength;
            }
            return statType;
        }
        public static string StatTypeToString(StatType statType)
        {
            string value = "0";

            if (statType==StatType.Leadership)
            {
                value = "0";
            }
            if (statType==StatType.Defense)
            {
                value = "1";
            }
            if (statType==StatType.Speed)
            {
                value = "2";
            }
            if (statType==StatType.Strength)
            {
                value = "4";
            }
            return value;
        }

        public static int GetItemsNumber(Hero hero)
        {
            int number = 0;
            Runtime.Log("Preparing to get number of equipments");
            if (hero.Item0.Id != null )
            {
                Runtime.Log("Increase the circle");
                number++;
            }
            if (hero.Item1.Id != null)
            {
                Runtime.Log("Increase the circle");
                number++;
            }
            if (hero.Item2.Id != null)
            {
                Runtime.Log("Increase the circle");
                number++;
            }
            if (hero.Item3.Id != null)
            {
                Runtime.Log("Increase the circle");
                number++;
            }
            if (hero.Item4.Id != null)
            {
                Runtime.Log("Increase the circle");
                number++;
            }
            Runtime.Log("Equipments number counted");
            return number;
        }

        public static Item GetItemByIndex(Hero hero, int i)
        {
            Runtime.Log("Get the Item");
            if (i == 0)
            {
                Runtime.Log("Get 1st Item");
                return hero.Item0;
            }
            if (i == 1)
            {
                Runtime.Log("Get 2st Item");
                return hero.Item0;
            }
            if (i == 2)
            {
                Runtime.Log("Get 3st Item");
                return hero.Item0;
            }
            if (i == 3)
            {
                Runtime.Log("Get 4st Item");
                return hero.Item0;
            }
            Runtime.Log("Get 5th Item");
            return hero.Item0;
        }
    }
}
