using Blitz.Cmn.Def;

namespace BlitzSniffer.Tracker.Player
{
    class Player
    {
        public bool IsActive
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public Team Team
        {
            get;
            set;
        }

        public bool IsAlive
        {
            get;
            set;
        }

        public Weapon Weapon
        {
            get;
            set;
        }

        public uint SpecialGaugeCharge
        {
            get;
            set;
        }

        public uint Kills
        {
            get;
            set;
        }

        public uint Deaths
        {
            get;
            set;
        }

        public uint Assists
        {
            get;
            set;
        }

        public Player(string name)
        {
            IsActive = false;
            Name = name;
            Team = Team.Neutral;
            IsAlive = false;
            Weapon = null;
            SpecialGaugeCharge = 0;
            Kills = 0;
            Deaths = 0;
            Assists = 0;
        }

    }

}
