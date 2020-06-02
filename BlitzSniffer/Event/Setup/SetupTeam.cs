using Blitz.Sead;
using System.Collections.Generic;

namespace BlitzSniffer.Event.Setup
{
    public class SetupTeam
    {
        public Color4f Color
        {
            get;
            set;
        }

        public List<SetupPlayer> Players
        {
            get;
            set;
        }

        public SetupTeam()
        {
            Players = new List<SetupPlayer>();
        }

    }
}
