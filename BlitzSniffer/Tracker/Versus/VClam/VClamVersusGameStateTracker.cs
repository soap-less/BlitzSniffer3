using Blitz.Cmn.Def;
using Nintendo.Sead;
using Syroot.BinaryData;

namespace BlitzSniffer.Tracker.Versus.VClam
{
    class VClamVersusGameStateTracker : GachiVersusGameStateTracker
    {
        public override VersusRule Rule => VersusRule.Vcl;

        public VClamVersusGameStateTracker(ushort stage, Color4f alpha, Color4f bravo) : base(stage, alpha, bravo)
        {

        }

        public override void Dispose()
        {

        }

        protected override void HandleSystemEvent(uint eventType, BinaryDataReader reader)
        {
            if (eventType == 6) // VClam finish
            {
                // These bytes are the "result left count", so convert them to score
                HandleFinishEvent((uint)100 - reader.ReadByte(), (uint)100 - reader.ReadByte());
            }
        }

    }
}
