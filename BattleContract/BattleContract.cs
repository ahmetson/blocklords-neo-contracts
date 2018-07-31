using BattleContract.Battle;
using BattleContract.Character;
using BattleContract.StorageLog;
using BattleContract.StorageData;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;
using BattleContract.Data;
using BattleContract.GameComponents;


/**
 *  Battle of the Blocklords game.
 *  
 *  Version: 1.0
 *  Author: Medet Ahmetson
 *
 * Battle Calculation for Hero VS. Hero: 
        * Involved in the Battle, the heroes, the number of troops, the items from both part
        * 
        * Passing Arguments
        * Hero #1 ID (13)              ARG #1
        * //Hero #1 Speed Stat (4)       
        * //Hero #1 Strength Stat (4)
        * Hero #1 Troops Number (4)    ARG #2
        * Hero #1 Item 1 Id (13)       ARG #3
        * Hero #1 Item 2 Id (13)       ARG #4
        * Hero #1 Item 3 Id (13)       ARG #5
        * Hero #1 Item 4 Id (13)       ARG #6
        * Hero #1 Item 5 Id (13)       ARG #7 
        * Hero #1 Address              ARG #15
        * Hero #2 ID (13)              ARG #8
        * //Hero #2 Speed Stat (4)
        * //Hero #2 Strength Stat (4)
        * //Hero #2 Defence Stat (4)
        * Hero #2 Troops Number (4)    ARG #9
        * Hero #2 Item 1 Id (13)       ARG #10
        * Hero #2 Item 2 Id (13)       ARG #11
        * Hero #2 Item 3 Id (13)       ARG #12
        * Hero #2 Item 4 Id (13)       ARG #13
        * Hero #2 Item 5 Id (13)       ARG #14
        * Hero #2 NPC (1)              ARG #16
        * Hero #2 Address              ARG #17
        * Battle ID (13)               ARG #18
        * 
        * 
        * Steps of the Battle:
        * 1) Calculate the Damage
        * 2) Attack to both of heroes
        * 3) Check who win or both lose
        * 4) If non of them win nor lose, repeat the step 1)
        * 5) If someone win the battle, go to step 6)
        * 6) Decide how many item stats will increase
        * 7) Calculate the increase amount
        * 8) Increase the stats
        * 
        * TOTAL LENGTH OF PASSING ARGUMENTS ARE
        * 
        * Involved Attacker's Speed, Strength, HP, Troops number
        * Involved Defenser's Speed, Strength, HP, Troops number and Defense
        * 
        */


namespace BattleContract
{
    class BattleContract : SmartContract
    {

        [Appcall("79ae73eb0f44b084769136790daa70e7efb7086e")]
        public static extern StorageContext GetItemContext(string operation, object[] args);

        [Appcall("ba22407851a6036b63e8c214481530ba45ee0605")]
        public static extern StorageContext GetHeroContext(string operation, object[] args);


        private static byte[] GetFalseByte(string message)
        {
            //Runtime.Log(message);
            BigInteger big = 1;
            return big.AsByteArray();
        }
        private static byte[] GetTrueByte(string message)
        {
            //Runtime.Log(message);
            BigInteger big = 1;
            return big.AsByteArray();
        }

        public static byte[] Main(string operation, object[] args)
        {
            // Changing the incoming arguments:
            /**
             * 0, 1         2, 3                4, 5                6, 7
             * Hero Id x2, Hero Parameters x2, Troops Number x2, Owners x2
             * 
             * 8                9       10 - 14, 15 - 19
             * Fighting NPC, battleId, Item Ids x2
             * 
             * Item ID, Item Value, Value Type x10
             */
            /*if (operation == "put" || 
                  operation == "update" ||
                  operation == "setItemState" || 
                  operation == "transfer" )
            {
                if (!Runtime.CheckWitness((byte[])args[0]))
                {
                    Runtime.Log("Authorization failed!");
                    return GetFalseByte();
                }
            }*/
            Runtime.Log("Version:0.1.9");

            if (operation == "attackHero")
            {
                //-----------------------------------------------
                // VALIDATOR 
                //-----------------------------------------------
                //if (!args.Length.Equals(18)) return GetFalseByte("required_18_arguments");
                //if (!IsValidHeroId((string)args[0])) return GetFalseByte("Attacker Hero's ID is invalid");
                //if (!IsValidHeroId((string)args[7])) return GetFalseByte("Attacked Hero's ID is invalid");

                //-----------------------------------------------
                // ATTACKER's 
                //-----------------------------------------------
                string attackerId = (string)args[0];

                string attackerParameters = (string)args[2];
                BigInteger attackerLeadership = HeroDataHelper.GetValue(HeroDataType.Leadership, attackerParameters);
                BigInteger attackerStrength = HeroDataHelper.GetValue(HeroDataType.Strength, attackerParameters);
                BigInteger attackerSpeed = HeroDataHelper.GetValue(HeroDataType.Speed, attackerParameters);
                BigInteger attackerIntelligence = HeroDataHelper.GetValue(HeroDataType.Intelligence, attackerParameters);
                BigInteger attackerDefense = HeroDataHelper.GetValue(HeroDataType.Defence, attackerParameters);

                Runtime.Log("Stats were taken for attacker!");

                int attackerClass = HeroDataHelper.GetClass(attackerParameters); //=Class
                int attackerTroops = (int)args[4];                              // Troops
                byte[] attackerOwner = (byte[])args[6];                         // Owner

                Runtime.Log("Prepare to get Items for attacker:");
                BigInteger[] attackerHead = ItemDataHelper.GetItem((string)args[10], (byte[])args[6]);         // Head= Leadership
                BigInteger[] attackerBody = ItemDataHelper.GetItem((string)args[11], (byte[])args[6]);         // Body = Defense
                BigInteger[] attackerHands = ItemDataHelper.GetItem((string)args[12], (byte[])args[6]);        // Hands = Speed
                BigInteger[] attackerWeapon = ItemDataHelper.GetItem((string)args[13], (byte[])args[6]);       // Weapon  = Strength
                BigInteger[] attackerShield = ItemDataHelper.GetItem((string)args[14], (byte[])args[6]);      // Shield = Defense
                //if (attacker.Items == null) return GetFalseByte();

                //-----------------------------------------------
                // DEFENDER's 
                //-----------------------------------------------
                string defenderId = (string)args[1];

                string defenderParameters = (string)args[3];
                BigInteger defenderLeadership = HeroDataHelper.GetValue(HeroDataType.Leadership, defenderParameters);
                BigInteger defenderStrength = HeroDataHelper.GetValue(HeroDataType.Strength, defenderParameters);
                BigInteger defenderSpeed = HeroDataHelper.GetValue(HeroDataType.Speed, defenderParameters);
                BigInteger defenderIntelligence = HeroDataHelper.GetValue(HeroDataType.Intelligence, defenderParameters);
                BigInteger defenderDefense = HeroDataHelper.GetValue(HeroDataType.Defence, defenderParameters);

                Runtime.Log("Stats were taken for attacked!");

                int defenderClass = HeroDataHelper.GetClass(defenderParameters); //=Class
                int defenderTroops = (int)args[5];                              // Troops
                byte[] defenderOwner = (byte[])args[7];                         // Owner

                Runtime.Log("Prepare to get Items for attacked:");

                BigInteger[] defenderHead = ItemDataHelper.GetItem((string)args[15], defenderOwner);         // Head
                BigInteger[] defenderBody = ItemDataHelper.GetItem((string)args[16], defenderOwner);         // Body
                BigInteger[] defenderHands = ItemDataHelper.GetItem((string)args[17], defenderOwner);        // Hands
                BigInteger[] defenderWeapon = ItemDataHelper.GetItem((string)args[18], defenderOwner);       // Weapon
                BigInteger[] defenderShield = ItemDataHelper.GetItem((string)args[19], defenderOwner);      // Shield
                //if (attacker.Items == null) return GetFalseByte();

                int npcMode = (int)args[8];
                string battleId = (string)args[9];

                Runtime.Log("Validation End, Start the Attacking!");

                //byte[] parameters = AttackHero(battleIdBytes, attacker, defender);
                int XP_1 = 500, XP_2 = 100;

                /*Runtime.Log("My Strength calculated");
                if (attackerWeapon.Length != 0)
                {
                    //Runtime.Log("My Strength 1: " + ((Item)attacker[12]).Stat.AsString());
                    attackerStrength = myStrength + myStrength1;
                }


                BigInteger myDefense = ((Stat)attacker[2]).Value.AsBigInteger();
                Runtime.Log("My Defense: " + ((Stat)attacker[2]).Value.AsString());
                if (attackerItem5.Id != null)
                {

                    BigInteger myDefense1 = ((Item)attacker[14]).Stat.AsBigInteger();
                    Runtime.Log("My Defense: " + ((Item)attacker[14]).Stat.AsString());
                    myDefense = myDefense + myDefense1;
                }
                if (attackerItem2.Id != null)
                {

                    BigInteger myDefense2 = ((Item)attacker[11]).Stat.AsBigInteger();
                    Runtime.Log("My Defense: " + ((Item)attacker[11]).Stat.AsString());
                    myDefense = myDefense + myDefense2;
                }


                BigInteger mySpeed = ((Stat)attacker[5]).Value.AsBigInteger();
                Runtime.Log("My Speed: " + ((Stat)attacker[5]).Value.AsString());
                if (attackerItem3.Id != null)
                {
                    BigInteger mySpeed1 = ((Item)attacker[12]).Stat.AsBigInteger();
                    Runtime.Log("My Speed: " + ((Item)attacker[12]).Stat.AsString());
                    mySpeed = mySpeed + mySpeed1;
                }       


                BigInteger enemyStrength = ((Stat)defender[4]).Value.AsBigInteger();
                Runtime.Log("Enemy Strength: " + ((Stat)defender[2]).Value.AsString());
                if (attackerItem4.Id != null)
                {
                    BigInteger enemyStrength1 = ((Item)attacker[12]).Stat.AsBigInteger();
                    //Runtime.Log("My Strength 1: " + defender.Item3.Stat.AsString());
                    enemyStrength = enemyStrength + enemyStrength1;
                }


                BigInteger enemyDefense = ((Stat)defender[2]).Value.AsBigInteger();
                Runtime.Log("Enemy Defense: " + ((Stat)defender[2]).Value.AsString());
                if (defenderItem5.Id != null)
                {
                    BigInteger enemyStrength1 = ((Item)defender[14]).Stat.AsBigInteger();
                    //Runtime.Log("My Strength 1: " + defender.Item4.Stat.AsString());
                    enemyDefense = enemyDefense + enemyStrength1;
                }
                if (defenderItem2.Id != null)
                {
                    BigInteger enemyStrength2 = ((Item)defender[11]).Stat.AsBigInteger();
                    //Runtime.Log("My Strength 1: " + defender.Item1.Stat.AsString());
                    enemyStrength = enemyStrength + enemyStrength2;
                }


                BigInteger enemySpeed = ((Stat)attacker[5]).Value.AsBigInteger();
                Runtime.Log("Enemy Speed: " + ((Stat)attacker[5]).Value.AsString());
                if (defenderItem3.Id != null)
                {
                    BigInteger enemySpeed1 = ((Item)attacker[12]).Stat.AsBigInteger();
                    //Runtime.Log("My Strength 1: " + defender.Item2.Stat.AsString());
                    enemySpeed = enemySpeed + enemySpeed1;
                }*/

                int myAdvantage = 1;

                //Runtime.Notify(attacker, defender);
                // Rider = 0, Archer = 1, Soldier = 2
                if (attackerClass == 0)
                {
                    if (defenderClass == 1) myAdvantage = 2;
                }
                if (attackerClass == 1)
                {
                    if (defenderClass == 2) myAdvantage = 2;
                }
                if (attackerClass == 2)
                {
                    if (defenderClass == 0) myAdvantage = 2;
                }
                // int myAdvantage = Character.Helper.GetAdvantage(attacker.Class, defender.Class);
                int enemyAdvantage = 1;
                if (myAdvantage != 2)
                 {
                     if (defenderClass == 0)
                     {
                         if (attackerClass == 1) enemyAdvantage = 2;
                     }
                     if (defenderClass == 1)
                     {
                         if (attackerClass == 2) enemyAdvantage = 2;
                     }
                     if (defenderClass == 2)
                     {
                         if (attackerClass == 0) enemyAdvantage = 2;
                     }
                 }
                //Runtime.Log("Attackers data analyzed");
                //int enemyAdvantage = Character.Helper.GetAdvantage(defender.Class, attacker.Class);

                //BigInteger myAttack = Battle.Helper.DamageCalculation(myStrength, mySpeed, enemyDefense, myAdvantage);
                //BigInteger enemyAttack = Battle.Helper.DamageCalculation(enemyStrength, enemySpeed, myDefense, enemyAdvantage);

                //int defensePart1 = 1 - (int)enemyDefense;
                //int defensePart2 = XP_1 + (int)enemyDefense;
                //int enemyCityDefense = 1;// - cityDefense;
                                          //int myDamage = (int)myStrength * (1 - (int)enemyDefense / (XP_1 + (int)enemyDefense)) * ((int)mySpeed / ((int)mySpeed + XP_2)) * myAdvantage * (1 - enemyCityDefense);
                BigInteger attackerDamage = attackerStrength * (1 - defenderDefense / (XP_1 + defenderDefense)) * (attackerSpeed / (attackerSpeed + XP_2)) * myAdvantage;
                if (attackerDamage == 0)
                {
                    Runtime.Log("Zero is result");
                } else {
                    Runtime.Log("Not Zero");
                }
                
                 //int defensePart12 = 1 - (int)myDefense;
                 //int defensePart22 = XP_1 + (int)myDefense;
                 BigInteger defenderDamage = defenderStrength * (1 - attackerDefense / (XP_1 + attackerDefense)) * (defenderSpeed / (defenderSpeed + XP_2)) * enemyAdvantage;

                 Runtime.Log("Damages calculated");

                 BigInteger myRemainTroops = Battle.Helper.AcceptDamage(defenderDamage, attackerTroops);
                 BigInteger enemyRemainTroops = Battle.Helper.AcceptDamage(attackerDefense, defenderTroops);

                 //Runtime.Log("Prepare For Logging");
                 /*byte[] enemyRemainBytes = enemyRemainTroops.AsByteArray();
                 //string enemyRemains = enemyRemainBytes.AsString();
                 Runtime.Log("Enemy remained: "+enemyRemainBytes.AsString());
                 byte[] myRemainBytes = new byte[8];
                 myRemainBytes = myRemainTroops.AsByteArray();
                 //string myRemains = myRemainBytes.AsString();
                 Runtime.Log("My remained troops " + myRemainBytes.AsString());
                 Runtime.Log("Damages are applied! Decide Battle answer!");*/
                 //Storage.Put(Storage.CurrentContext, battleId, "2");
                 
                BigInteger battleResult = Battle.Helper.CalculateBattleResult(attackerTroops, myRemainTroops, defenderTroops, enemyRemainTroops);
                string battleResultString = StringAndArray.Helper.GetStringByDigit(battleResult);
                byte[] battleResultBytes = battleResultString.AsByteArray();

                string battleType = "1";// Battle.Helper.GetBattleType(attacker.IsNPC, defender.IsNPC);
                if (npcMode == 1)
                {
                    battleType = "2";
                }
                byte[] battleTypeBytes = battleType.AsByteArray();

                Runtime.Log("Battle Answer Decided! Log the answer");

                //Runtime.Log(">Battle Result " + battleResultString);
                //Runtime.Log(">Battle Type " + battleTypeString);

                //-----------------------------------
                // LOG the result
                //-----------------------------------
                //byte[] parameters = battleResult;
                //Runtime.Log("Parameter #1: "  + battleId + parameters.AsString());
                /*
                byte[] myIdBytes = my.Id.AsByteArray();
                byte[] parameters2 = parameters.Concat(myIdBytes);
                Runtime.Log(parameters2.AsString());

                byte[] parameters3 = parameters2.Concat(myRemainBytes);
                Runtime.Log(parameters3.AsString());

                byte[] enemyIdBytes = enemy.Id.AsByteArray();
                byte[] parameters4 = parameters2.Concat(enemyIdBytes);
                Runtime.Log(parameters4.AsString());

                /*byte[] parameters5 = new byte[72];
                parameters5 = parameters4.Concat(enemyRemainBytes);
                Runtime.Log(parameters5.AsString());
               /*increasingNumber + GetIncreasingsAsLogParameter(battleLog) + 

               byte[] parameters6 = new byte[112];
               parameters6 = parameters5.Concat(my.Owner);
               Runtime.Log(parameters6.AsString());

               byte[] parameters7 = new byte[152];
               parameters7 = parameters6.Concat(enemy.Owner);
               Runtime.Log(parameters7.AsString());*/

                Storage.Put(Storage.CurrentContext, battleId, battleResultBytes);

                if (battleResult != 2)  // Both Lose
                {
                    Runtime.Log("Someone won the Battle!");
                    return new byte[1] { 1 };
                }
                else
                {
                    Runtime.Log("Both lose the battle");
                }/*

                Runtime.Log("Some of the fighters won the battle, reward him!");
                return RewardWinner(battleLog, my, enemy);*/
                //return parameters;
                //Runtime.Log("Parameters of Battle returned" + battleId + ", "+parameters.AsString());
                //Runtime.Notify(battleId, parameters);
                //Storage.Put(Storage.CurrentContext, battleId, battleResultString);

            }

            // @Param Hero ID
            if (operation == "attackCity") return AttackCity((string)args[0]);
            return new byte[1] { 0 };
            //return GetFalseByte("command_not_found");
        }

        private static byte[] AttackCity(string itemId)
        {
            // Validate input
            if (!IsValidHeroId(itemId))
            {
                Runtime.Log("Invalid Item ID parameter!");
                return GetFalseByte("invalid_item_id");
            }
            byte[] item = Storage.Get(Storage.CurrentContext, itemId);
            Runtime.Log("Item is " + item.AsString());
            return item;
        }

        
        private static byte[] AttackHero(byte[] battleId, Hero my, Hero enemy)
        {
            BigInteger XP_1 = 500, XP_2 = 100;
            
            BigInteger myStrength = my.Strength.Value.AsBigInteger() + my.Item3.Stat.AsBigInteger();
            BigInteger myDefense = my.Defense.Value.AsBigInteger() + my.Item4.Stat.AsBigInteger() + my.Item1.Stat.AsBigInteger();
            BigInteger mySpeed = my.Speed.Value.AsBigInteger() + my.Item1.Stat.AsBigInteger();
            BigInteger enemyStrength = enemy.Strength.Value.AsBigInteger() + enemy.Item3.Stat.AsBigInteger();
            BigInteger enemyDefense = enemy.Defense.Value.AsBigInteger() + enemy.Item4.Stat.AsBigInteger() + enemy.Item1.Stat.AsBigInteger();
            BigInteger enemySpeed = enemy.Speed.Value.AsBigInteger() + enemy.Item1.Stat.AsBigInteger();

            int myAdvantage = Character.Helper.GetAdvantage(my.Class, enemy.Class);
            int enemyAdvantage = Character.Helper.GetAdvantage(enemy.Class, my.Class);

            

            //BigInteger myAttack = Battle.Helper.DamageCalculation(myStrength, mySpeed, enemyDefense, myAdvantage);
            //BigInteger enemyAttack = Battle.Helper.DamageCalculation(enemyStrength, enemySpeed, myDefense, enemyAdvantage);

            BigInteger speed = (mySpeed / (mySpeed + XP_2));
            BigInteger defensePart1 = 1 - enemyDefense;
            BigInteger defensePart2 = XP_1 + enemyDefense;
            BigInteger defense = defensePart1 / defensePart2;
            BigInteger cityDefense = 1;// - cityDefense;
            BigInteger myDamage = myStrength * defense * speed * myAdvantage * cityDefense;

            BigInteger speed2 = (enemySpeed / (enemySpeed + XP_2));
            BigInteger defensePart12 = 1 - myDefense;
            BigInteger defensePart22 = XP_1 + myDefense;
            BigInteger defense2 = defensePart12 / defensePart22;
            BigInteger cityDefense2 = 1;// - cityDefense;
            BigInteger enemyDamage = enemyStrength * defense2 * speed2 * enemyAdvantage * cityDefense2;

            Runtime.Log("Damage will be calculated");

            BigInteger myRemainTroops = Battle.Helper.AcceptDamage(myDamage, my.Troops);
            BigInteger enemyRemainTroops = Battle.Helper.AcceptDamage(myDamage, enemy.Troops);

            

            Runtime.Log("Prepare For Logging");
            /*byte[] enemyRemainBytes = enemyRemainTroops.AsByteArray();
            //string enemyRemains = enemyRemainBytes.AsString();
            Runtime.Log("Enemy remained: "+enemyRemainBytes.AsString());
            byte[] myRemainBytes = new byte[8];
            myRemainBytes = myRemainTroops.AsByteArray();
            //string myRemains = myRemainBytes.AsString();
            Runtime.Log("My remained troops " + myRemainBytes.AsString());
            Runtime.Log("Damages are applied! Decide Battle answer!");*/

            BigInteger battleResult = Battle.Helper.CalculateBattleResult(my.Troops, myRemainTroops, enemy.Troops, enemyRemainTroops);
            string battleResultString = StringAndArray.Helper.GetStringByDigit(battleResult);
            byte[] battleResultBytes = battleResultString.AsByteArray();

            int battleType = Battle.Helper.GetBattleType(my.IsNPC, enemy.IsNPC);
            string battleTypeString = StringAndArray.Helper.GetStringByDigit(battleType);
            byte[] battleTypeBytes = battleTypeString.AsByteArray();
            
            Runtime.Log("Battle Answer Decided! Log the answer");

            //Runtime.Log(">Battle Result " + battleResultString);
            //Runtime.Log(">Battle Type " + battleTypeString);

            byte[] parameters = battleResultBytes.Concat(battleTypeBytes);
            Runtime.Log("Parameter #1: " + parameters.AsString());
            /*
            byte[] myIdBytes = my.Id.AsByteArray();
            byte[] parameters2 = parameters.Concat(myIdBytes);
            Runtime.Log(parameters2.AsString());

            byte[] parameters3 = parameters2.Concat(myRemainBytes);
            Runtime.Log(parameters3.AsString());

            byte[] enemyIdBytes = enemy.Id.AsByteArray();
            byte[] parameters4 = parameters2.Concat(enemyIdBytes);
            Runtime.Log(parameters4.AsString());

            /*byte[] parameters5 = new byte[72];
            parameters5 = parameters4.Concat(enemyRemainBytes);
            Runtime.Log(parameters5.AsString());
            /*increasingNumber + GetIncreasingsAsLogParameter(battleLog) + 

            byte[] parameters6 = new byte[112];
            parameters6 = parameters5.Concat(my.Owner);
            Runtime.Log(parameters6.AsString());

            byte[] parameters7 = new byte[152];
            parameters7 = parameters6.Concat(enemy.Owner);
            Runtime.Log(parameters7.AsString());*/

            /*if (battleResult == BattleResult.BOTH_LOSE)
            {
                Runtime.Log("Log the Result");
                //Storage.Put(Storage.CurrentContext, "1", "2");
                //StorageLog.Helper.LogBattleResult(battleLog, my, enemy);
                Runtime.Log("both_lose");
                return parameters;

            }
            
            Runtime.Log("Some of the fighters won the battle, reward him!");
            return RewardWinner(battleLog, my, enemy);*/
            return parameters;
        }
        public static int GetItemsNumber(Hero hero)
        {
            int number = 0;
            Runtime.Log("Preparing to get number of equipments");
            if (hero.Item0.Id != null)
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
        

        public static byte[] RewardWinner(BattleLog battleLog, Hero my, Hero enemy)
        {
            Hero winner = my, loser = enemy;

            if (battleLog.battleResult.Equals( StringAndArray.Helper.GetStringByDigit((int) BattleResult.ENEMY_WIN)))
            {
                winner = enemy;
                loser = my;
            }

            int[] randomItemIndexes = new int[0];
            if (!winner.IsNPC)
            {
                randomItemIndexes = Math.Helper.SelectRandomNumbers(Character.Helper.GetItemsNumber(winner));
                ItemIncreasing[] itemIncreasings = IncreasingTable.Get();
                int battleType = Battle.Helper.GetBattleType(winner.IsNPC, loser.IsNPC);
                battleLog.battleType = StringAndArray.Helper.GetStringByDigit(battleType);
                Runtime.Log("Battle Type is " + battleLog.battleType);
                for (int i = 0; i < randomItemIndexes.Length; i++)
                {
                    Item item = Character.Helper.GetItemByIndex(winner, i);
                    int increaseValue = Battle.Helper.GetIncreaseValue(itemIncreasings, item, battleType);

                    StorageLog.Helper.AddIncreasedItem(battleLog, item.Id, item.Stat.AsString(), increaseValue);

                    Battle.Helper.IncreaseStat(item, increaseValue, winner.Owner);
                }
            }

            StorageLog.Helper.LogBattleResult(battleLog, my, enemy);
            return GetTrueByte("Reward the Battle");
        }

        

        /**
         *  Checks the item id. Item ID's length should be exactly 15.
         *  First 13 is represents the Unix timestamp in Milliseconds.
         *  And last 2 digits are representing the random number, just for case.
         */
        private static bool IsValidHeroId(string itemId)
        {
            return itemId.Length.Equals(HeroDataHelper.IdLength);
        }
    }
}
