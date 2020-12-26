using Blitz.Cmn.Def;
using BlitzSniffer.Clone;
using BlitzSniffer.Event;
using BlitzSniffer.Event.Versus;
using BlitzSniffer.Util;
using Nintendo.Sead;
using Syroot.BinaryData;
using System.IO;

namespace BlitzSniffer.Tracker.Versus.VGoal
{
    class VGoalVersusGameStateTracker : GachiVersusGameStateTracker
    {
        public override VersusRule Rule => VersusRule.Vgl;

        private uint GachihokoTimeout;

        public VGoalVersusGameStateTracker(ushort stage, Color4f alpha, Color4f bravo) : base(stage, alpha, bravo)
        {
            GachihokoTimeout = 0;

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

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                BitReader bitReader = new BitReader(reader);

                uint hokoTimeout = bitReader.ReadVariableBits(16);

                if (hokoTimeout != GachihokoTimeout)
                {
                    GachihokoTimeout = hokoTimeout;
                }

                uint ReadScore()
                {
                    return (uint)((bitReader.ReadF32Bit(2, 14) - 1.0f) * 100.0f);
                }

                uint bravoBestScore = ReadScore(); // yes, bravo first
                uint alphaBestScore = ReadScore();

                if (alphaBestScore != AlphaScore || bravoBestScore != BravoScore)
                {
                    AlphaScore = alphaBestScore;
                    BravoScore = bravoBestScore;

                    EventTracker.Instance.AddEvent(new GachiScoreUpdateEvent()
                    {
                        AlphaScore = AlphaScore,
                        BravoScore = BravoScore
                    });
                }

                // Current position
                // float position = bitReader.ReadF32Bit(3, 14) - 2.0f;
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
