using Nintendo.Sead;

namespace BlitzSniffer.Tracker.Versus
{
    abstract class GachiVersusGameStateTracker : VersusGameStateTracker
    {
        public uint AlphaScore
        {
            get;
            protected set;
        }

        public uint BravoScore
        {
            get;
            protected set;
        }

        public GachiVersusGameStateTracker(ushort stage, Color4f alpha, Color4f bravo) : base(stage, alpha, bravo)
        {
            AlphaScore = 0;
            BravoScore = 0;
        }

    }
}
