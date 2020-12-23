using Blitz.Cmn.Def;
using BlitzSniffer.Clone;
using Nintendo.Sead;
using Syroot.BinaryData;

namespace BlitzSniffer.Tracker.Versus.VGoal
{
    class VGoalVersusGameStateTracker : GachiVersusGameStateTracker
    {
        public override VersusRule Rule => VersusRule.Vgl;

        public VGoalVersusGameStateTracker(ushort stage, Color4f alpha, Color4f bravo) : base(stage, alpha, bravo)
        {
            CloneHolder holder = CloneHolder.Instance;
            holder.RegisterClone(121);
            holder.CloneChanged += HandleGachihokoState;
            holder.CloneChanged += HandleGachihokoEvent;
        }

        public override void Dispose()
        {
            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged -= HandleGachihokoState;
            holder.CloneChanged -= HandleGachihokoEvent;
        }

        private void HandleGachihokoState(object sender, CloneChangedEventArgs args)
        {
            if (args.CloneId != 121 || args.ElementId != 2)
            {
                return;
            }
        }

        private void HandleGachihokoEvent(object sender, CloneChangedEventArgs args)
        {
            if (args.CloneId != 121 || args.ElementId != 3)
            {
                return;
            }

            /*
             * BreakBarrier
             * GetGachihoko
             * Lost
             * LostToInit
             * Recovery
             * Reallocate
             * ExpiredBlast
             */
        }

        protected override void HandleSystemEvent(uint eventType, BinaryDataReader reader)
        {

        }

    }
}
