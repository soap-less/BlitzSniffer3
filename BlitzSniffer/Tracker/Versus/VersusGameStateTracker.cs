using Blitz.Cmn.Def;
using Blitz.Sead;

namespace BlitzSniffer.Tracker.Versus
{
    abstract class VersusGameStateTracker : GameStateTracker
    {
        public VersusRule Rule
        {
            get;
            private set;
        }

        public VersusGameStateTracker(ushort stage, ushort rule, Color4f alpha, Color4f bravo) : base(stage, alpha, bravo)
        {
            Rule = (VersusRule)rule;
        }

    }
}
