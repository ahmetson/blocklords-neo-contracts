using BattleContract.Character;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using System.Numerics;

namespace BattleContract.StorageData
{
    class HeroDataHelper
    {
        public static readonly int bytes = 1;

        public static int GetIndex(HeroDataType type)
        {
            int i = 0;  // index
            if (type==HeroDataType.Leadership)
                i = 0;
            if (type==HeroDataType.Strength)
                i = 4;
            if (type==HeroDataType.Speed)
                i = 8;
            if (type==HeroDataType.Intelligence)
                i = 12;
            if (type==HeroDataType.Defence)
                i = 16;
            if (type==HeroDataType.Nation)
                i = 20;
            if (type==HeroDataType.Class)
                i = 21;
            if (type==HeroDataType.OptionalData)
                i = 22;
            if (type==HeroDataType.Address)
                i = 23;
            return i * bytes;
        }
        public static int GetLength(HeroDataType type)
        {
            int i = 0;  // Length
            if (type==HeroDataType.Leadership)
                i = 4;
            if (type==HeroDataType.Strength)
                i = 4;
            if (type==HeroDataType.Speed)
                i = 4;
            if (type==HeroDataType.Intelligence)
                i = 4;
            if (type==HeroDataType.Defence)
                i = 4;
            if (type==HeroDataType.Nation)
                i = 1;
            if (type==HeroDataType.Class)
                i = 1;
            if (type==HeroDataType.OptionalData)
                i = 1;
            if (type==HeroDataType.Address)
                i = 20;
            return i;
        }


        public static readonly int IdLength = 13;

        public static BigInteger GetValue(HeroDataType type, string parameters)
        {
            byte[] p = parameters.Substring(GetIndex(type), GetLength(type)).AsByteArray();
            return p.AsBigInteger();
        }

        public static int GetClass(string parameters)
        {
            HeroDataType type = HeroDataType.Class;
            string param = parameters.Substring(GetIndex(type), GetLength(type));

            return Character.Helper.StringToClassType(param);
        }

        public static string GetAddress(string parameters)
        {
            HeroDataType type = HeroDataType.Address;
            return parameters.Substring(GetIndex(type), GetLength(type));
        }

        public static readonly int HeroDataLength = 43;

        public static object[] GetHeroData(string heroId, byte[] address)
        {
            StorageContext storageContext = BattleContract.GetHeroContext("getStorage", new object[] { });
            byte[] parametersBytes = Storage.Get(storageContext, heroId.AsByteArray());

            string parameters = parametersBytes.AsString(); //.HexToBytes().ToString();

            Runtime.Log("Parameters are ");
            Runtime.Log(parameters);


            if (!parameters.Length.Equals(HeroDataHelper.HeroDataLength))
            {
                Runtime.Log("Parameters Length is not valid");
                return new object[0];
            }

            string heroOwner = HeroDataHelper.GetAddress(parameters);
            if (!heroOwner.AsByteArray().Equals(address))
            {
                Runtime.Log("Player is not the owner of the Hero");
                return new object[0];
            }
            Runtime.Log("Player is owner of Hero");

            object[] data = new object[10];
            data[0] = heroId;

            /*Stat stat0 = new Stat();
            stat0.statType = Character.Helper.StatTypeToString(StatType.Leadership);
            stat0.Value = HeroDataHelper.GetValue(HeroDataType.Leadership, parameters);
            Stat stat1 = new Stat();
            stat1.statType = Character.Helper.StatTypeToString(StatType.Strength);
            stat1.Value = HeroDataHelper.GetValue(HeroDataType.Strength, parameters);
            Stat stat2 = new Stat();
            stat2.statType = Character.Helper.StatTypeToString(StatType.Speed);
            stat2.Value = HeroDataHelper.GetValue(HeroDataType.Speed, parameters);
            Stat stat3 = new Stat();
            stat3.statType = Character.Helper.StatTypeToString(StatType.Intelligence);
            stat3.Value = HeroDataHelper.GetValue(HeroDataType.Intelligence, parameters);
            Stat stat4 = new Stat();
            stat4.statType = Character.Helper.StatTypeToString(StatType.Defense);
            stat4.Value = HeroDataHelper.GetValue(HeroDataType.Defence, parameters);
            Runtime.Log("Last of Arrays");
            //hero.Class = HeroDataHelper.GetClass(parameters);

            data[1] = stat0;            // Leadership
            data[2] = stat4;            // Defense
            data[3] = stat3;            // Intelligence
            data[4] = stat1;            // Strength
            data[5] = stat2;            // Speed

            Runtime.Log("Stats were takken");

            data[6] = HeroDataHelper.GetClass(parameters);//=Class
            data[7] = 0;                // Troops
            data[8] = false;            // is NPC
            data[9] = address;          // Owner

            /*Runtime.Log("Start to init the Items");
            data[10] = null;            // Head             = Leadership
            data[11] = null;            // Body             = Defense
            data[12] = null;            // Hands            = Speed
            data[13] = null;            // Weapon           = Strength
            data[14] = null;            // Shield           = Defense*/

            /// -------------- VARIANT OF DATA RETURN WITH CLASS --------------------
            /*Hero hero = new Hero();
            Runtime.Log("Player Hero Created");
            hero.Id = heroId;
            Runtime.Log("Assigned Hero ID");
            Stat stat0 = new Stat();
            stat0.statType = Character.Helper.StatTypeToString(StatType.Leadership);
            stat0.Value = HeroDataHelper.GetValue(HeroDataType.Leadership, parameters);
            Stat stat1 = new Stat();
            stat1.statType = Character.Helper.StatTypeToString( StatType.Strength);
            stat1.Value = HeroDataHelper.GetValue(HeroDataType.Strength, parameters);
            Stat stat2 = new Stat();
            stat2.statType = Character.Helper.StatTypeToString(StatType.Speed);
            stat2.Value = HeroDataHelper.GetValue(HeroDataType.Speed, parameters);
            Stat stat3 = new Stat();
            stat3.statType = Character.Helper.StatTypeToString(StatType.Intelligence);
            stat3.Value = HeroDataHelper.GetValue(HeroDataType.Intelligence, parameters);
            Stat stat4 = new Stat();
            stat4.statType = Character.Helper.StatTypeToString(StatType.Defense);
            stat4.Value = HeroDataHelper.GetValue(HeroDataType.Defence, parameters);
            Runtime.Log("Last of Arrays");
            hero.Class = HeroDataHelper.GetClass(parameters);

            hero.Leadership = stat0;
            hero.Defense = stat4;
            hero.Intelligence = stat3;
            hero.Strength = stat1;
            hero.Speed = stat2;*/

            Runtime.Log("Hero data takken successfully!");
            return data;
        }
    }
}
