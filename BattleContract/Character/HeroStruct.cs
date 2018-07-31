using BattleContract.GameComponents;

namespace BattleContract.Character
{
    public struct Hero
    {
        public Stat Leadership;
        public Stat Defense;
        public Stat Speed;
        public Stat Intelligence;
        public Stat Strength;

        public string Id;
        public int Class;
        public int Troops;
        public Item Item0;              // Head             = Leadership
        public Item Item1;              // Body             = Defense
        public Item Item2;              // Hands            = Speed
        public Item Item3;              // Weapon           = Strength
        public Item Item4;              // Shield           = Defense
        public bool IsNPC;

        public byte[] Owner;
    }
}
