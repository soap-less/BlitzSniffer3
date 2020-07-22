using Nintendo.Sead;

namespace BlitzSniffer.Tracker.Versus
{
    abstract class VersusGameStateTracker : GameStateTracker
    {
        public VersusGameStateTracker(ushort stage, Color4f alpha, Color4f bravo) : base(stage, alpha, bravo)
        {
        }

    }
}
