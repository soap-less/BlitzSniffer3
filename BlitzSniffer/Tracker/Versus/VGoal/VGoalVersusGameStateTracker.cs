using Blitz.Cmn.Def;
using BlitzSniffer.Clone;
using BlitzSniffer.Event;
using BlitzSniffer.Event.Player.VGoal;
using BlitzSniffer.Event.Versus;
using BlitzSniffer.Event.Versus.VGoal;
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

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                byte eventType = reader.ReadByte();

                switch (eventType)
                {
                    case 0: // BreakBarrier
                        byte senderPlayerIdx = reader.ReadByte();
                        byte breakerPlayerIdx = reader.ReadByte();

                        // reader.Seek(-4, SeekOrigin.End);
                        // uint breakFrame = reader.ReadUInt32();

                        EventTracker.Instance.AddEvent(new VGoalBarrierBreakEvent()
                        {
                            BreakerPlayerIdx = breakerPlayerIdx
                        });

                        break;
                    case 1: // GetGachihoko
                        reader.Seek(1); // nothing

                        byte heldPlayer = reader.ReadByte();

                        // reader.Seek(-4, SeekOrigin.End);
                        // uint getFrame = reader.ReadUInt32();

                        Player.Player responsiblePlayer = GameSession.Instance.PlayerTracker.GetPlayer(heldPlayer);
                        responsiblePlayer.HasGachihoko = true;

                        EventTracker.Instance.AddEvent(new PlayerGetGachihokoEvent()
                        {
                            PlayerIdx = heldPlayer
                        });

                        break;
                    case 2: // Lost
                    case 3: // LostToInit
                        // TODO: What is the difference between these two?

                        // The player only ever loses the Gachihoko upon death, so we let PlayerTracker fire a
                        // PlayerLostGachihoko event while processing their death instead of firing it here.

                        // Reset Gachihoko timeout
                        GachihokoTimeout = 0;

                        break;
                    case 4: // Recovery
                        // TODO: What's this?

                        break;
                    case 5: // Reallocate
                        // TODO: What's this?

                        break;
                    case 6: // ExpiredBlast
                        // TODO: For this event, the name seems to indicate that this fires upon the Gachihoko's
                        // timer running out, but it seems to appear every time the Gachihoko is lost???

                        break;
                }

            }
        }

        protected override void HandleSystemEvent(uint eventType, BinaryDataReader reader)
        {
            if (eventType == 5) // VGoal finish
            {
                // These ushorts are the "result left count", so convert them to score
                EventTracker.Instance.AddEvent(new GachiFinishEvent()
                {
                    AlphaScore = (uint)100 - reader.ReadUInt16(),
                    BravoScore = (uint)100 - reader.ReadUInt16()
                });
            }
        }

    }
}
