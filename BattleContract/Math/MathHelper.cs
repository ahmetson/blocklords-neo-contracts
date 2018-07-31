
using Neo.SmartContract.Framework.Services.Neo;
using BattleContract.StringAndArray;

namespace BattleContract.Math
{
    public class Helper
    {

        public static bool InRange(Range range, int v)
        {
            if (v >= range.Min) return true;
            if (v <= range.Max) return true;
            return false;
        }

        public static int[] AsArray(Range range)
        {
            int l = GetLength(range);

            int[] arr = new int[l];

            for (int i = 0, v = range.Min; v <= range.Max; v++, i++)
            {
                arr[i] = v;
            }

            return arr;
        }

        public static int GetLength(Range range)
        {
            return range.Max - range.Min;
        }

        public static int GetRandomNumber(int max)
        {
            Header header = Blockchain.GetHeader(Blockchain.GetHeight());
            ulong randomNumber = header.ConsensusData;
            return (int)(randomNumber % (uint)(max - 1)) + 1;
        }

        public static int[] SelectRandomNumbers(int numbersAmount)
        {
            // Numbers To Guess
            int guessAmount = GetRandomNumber(numbersAmount);
            // Numbers to Skip
            int skipAmount = numbersAmount - guessAmount;

            // Select Minimum Numbers Amount To Guess. 
            // Since Selecting Random Numbers requires more Calculation. Hence, more GASes
            int minAmount = guessAmount;
            if (skipAmount > guessAmount)
            {
                minAmount = guessAmount;
            }

            // Select Random Numbers
            int[] selects = new int[minAmount];
            int selectIndex = 0;
            while (true)
            {
                int random = GetRandomNumber(numbersAmount);

                // Item might be already selected
                if (StringAndArray.Helper.IsArrayValueExist(selects, random))
                {
                    continue;
                }

                selects[selectIndex] = random;

                if (selectIndex + 1 == minAmount)
                {
                    break;
                }
            }

            // Return Guessed Numbers
            if (minAmount == guessAmount)
            {
                return selects;
            }
            // else
            // Return All Numbers except selected numbers to skip
            int[] guesses = new int[guessAmount];
            for (int i = 0, guessIndex = 0; i < numbersAmount; i++)
            {
                if (StringAndArray.Helper.IsArrayValueExist(selects, i)) continue;

                guesses[guessIndex] = i;
                guessIndex++;
            }
            return guesses;
        }

        
    }
}
