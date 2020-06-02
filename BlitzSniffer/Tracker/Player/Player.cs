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

        public int SpecialGaugeCharge
        {
            get;
            set;
        }

        public int Kills
        {
            get;
            set;
        }

        public int Deaths
        {
            get;
            set;
        }

        public int Assists
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
