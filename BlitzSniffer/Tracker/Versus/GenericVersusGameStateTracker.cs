using Blitz.Cmn.Def;
using Nintendo.Sead;

namespace BlitzSniffer.Tracker.Versus
{
    class GenericVersusGameStateTracker : VersusGameStateTracker
    {
        public VersusRule Rule
        {
            get;
            private set;
        }

        public GenericVersusGameStateTracker(ushort stage, VersusRule rule, Color4f alpha, Color4f bravo) : base(stage, alpha, bravo)
        {
            Rule = rule;
        }

        public override void Dispose()
        {

        }

    }
}
