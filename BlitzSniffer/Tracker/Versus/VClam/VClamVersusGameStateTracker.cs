using Blitz.Cmn.Def;
using BlitzSniffer.Clone;
using Nintendo.Sead;
using Syroot.BinaryData;
using System.IO;

namespace BlitzSniffer.Tracker.Versus.VClam
{
    class VClamVersusGameStateTracker : GachiVersusGameStateTracker
    {
        public override VersusRule Rule => VersusRule.Vcl;

        public VClamVersusGameStateTracker(ushort stage, Color4f alpha, Color4f bravo) : base(stage, alpha, bravo)
        {
            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged += HandleScoreEvent;

            holder.RegisterClone(134);
        }

        public override void Dispose()
        {
            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged -= HandleScoreEvent;
        }

        /* Master:
         * 
         * Take
         * ClamSpawn
         * Result
         * BasketBreak
         * BasketRepair
         * ReserveThrow
         * Sleep
         * Score
         */

        private void HandleScoreEvent(object sender, CloneChangedEventArgs args)
        {
            if (args.CloneId != 134)
            {
                return;
            }

            if (args.ElementId != 8)
            {
                return;
            }

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                uint gameFrame = reader.ReadUInt32();
                uint basketLeft = reader.ReadUInt16();

                uint newAlphaScore = reader.ReadByte();
                uint newBravoScore = reader.ReadByte();
                uint newAlphaPenalty = reader.ReadByte();
                uint newBravoPenalty = reader.ReadByte();

                UpdateScores(newAlphaScore, newBravoScore, newAlphaPenalty, newBravoPenalty);
            }
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
