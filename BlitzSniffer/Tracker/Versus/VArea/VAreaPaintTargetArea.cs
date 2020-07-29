using Blitz.Cmn.Def;
using BlitzSniffer.Event;
using BlitzSniffer.Event.Versus.VArea;

namespace BlitzSniffer.Tracker.Versus.VArea
{
    class VAreaPaintTargetArea
    {
        // TODO: is there a bprm or something where this information can be loaded from?
        private static readonly float NEUTRALIZE_PERCENT = 0.5f;
        private static readonly float CAPTURE_PERCENT = 0.7f;

        private int AreaIdx
        {
            get;
            set;
        }

        public Team ControllingTeam
        {
            get;
            private set;
        }

        public float PaintPercentage
        {
            get;
            private set;
        }

        public byte Unk2
        {
            get;
            private set;
        }

        public sbyte Unk3
        {
            get;
            private set;
        }

        public VAreaPaintTargetArea(int idx)
        {
            AreaIdx = idx;
            ControllingTeam = Team.Neutral;
            PaintPercentage = 0;
            Unk2 = 0;
            Unk3 = 0;
        }

        public void ChangeControl(Team team)
        {
            if (ControllingTeam != team)
            {
                EventTracker.Instance.AddEvent(new VAreaPaintAreaControlChangeEvent()
                {
                    AreaIdx = AreaIdx,
                    Team = (uint)team
                });
            }

            ControllingTeam = team;
        }

        public void UpdateWithState(float paintPercent, byte unk2, sbyte unk3)
        {
            if (ControllingTeam == Team.Alpha)
            {
                // Alpha paint percentage is always positive
                if (paintPercent <= 0)
                {
                    return;
                }

                if (PaintPercentage == paintPercent)
                {
                    return;
                }

                PaintPercentage = paintPercent;

                EventTracker.Instance.AddEvent(new VAreaPaintAreaCappedStateUpdateEvent()
                {
                    AreaIdx = AreaIdx,
                    ToNeutralize = PaintPercentage - NEUTRALIZE_PERCENT
                });
            }
            else if (ControllingTeam == Team.Bravo)
            {
                // Bravo paint percentage is always negative
                if (paintPercent >= 0)
                {
                    return;
                }

                if ((PaintPercentage * -1) == paintPercent)
                {
                    return;
                }

                PaintPercentage = paintPercent * -1;

                EventTracker.Instance.AddEvent(new VAreaPaintAreaCappedStateUpdateEvent()
                {
                    AreaIdx = AreaIdx,
                    ToNeutralize = PaintPercentage - NEUTRALIZE_PERCENT
                });
            }
            else if (ControllingTeam == Team.Neutral)
            {
                float newPaintPercent = paintPercent;

                Team favouredTeam;
                if (paintPercent > 0)
                {
                    favouredTeam = Team.Alpha;
                }
                else if (paintPercent < 0)
                {
                    favouredTeam = Team.Bravo;
                    newPaintPercent *= -1; // make positive
                }
                else
                {
                    favouredTeam = Team.Neutral;
                }

                if (PaintPercentage == newPaintPercent)
                {
                    return;
                }

                PaintPercentage = newPaintPercent;

                EventTracker.Instance.AddEvent(new VAreaPaintAreaContestedStateUpdateEvent()
                {
                    AreaIdx = AreaIdx,
                    FavouredTeam = favouredTeam,
                    ToCapture = CAPTURE_PERCENT - PaintPercentage
                });
            }

            Unk2 = unk2;
            Unk3 = unk3;
        }

    }
}
