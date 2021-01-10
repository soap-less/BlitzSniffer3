using Blitz.Cmn.Def;
using BlitzSniffer.Event;
using BlitzSniffer.Event.Versus.VClam;
using System;

namespace BlitzSniffer.Tracker.Versus.VClam
{
    class VClamBasket : IDisposable
    {
        public Team OwningTeam
        {
            get;
            private set;
        }

        public bool IsBroken
        {
            get;
            private set;
        }

        public uint BreakTick
        {
            get;
            private set;
        }

        public uint LeftBrokenFrames
        {
            get;
            private set;
        }

        public uint RepairTick
        {
            get;
            private set;
        }

        public VClamBasket(Team team)
        {
            OwningTeam = team;

            Reset();

            GameSession.Instance.GameTicked += HandleGameTick;
        }

        public void Dispose()
        {
            GameSession.Instance.GameTicked -= HandleGameTick;
        }

        public void RequestBreak(uint tick)
        {
            BreakTick = tick;
        }

        public void NullifyBreakRequest()
        {
            BreakTick = 0;
        }

        public void UpdateBrokenFrames(uint newFrames)
        {
            if (newFrames != LeftBrokenFrames)
            {
                LeftBrokenFrames = newFrames;

                EventTracker.Instance.AddEvent(new VClamBasketTimeoutUpdateEvent()
                {
                    Team = OwningTeam,
                    Timeout = newFrames
                });
            }
        }

        public void RequestRepair(uint tick)
        {
            RepairTick = tick;
        }

        private void Reset()
        {
            IsBroken = false;
            BreakTick = 0;
            LeftBrokenFrames = 0; // set directly to avoid event firing
            RepairTick = 0;
        }

        private void HandleGameTick(object sender, GameTickedEventArgs args)
        {
            if (!IsBroken)
            {
                if (BreakTick == 0)
                {
                    return;
                }

                if (args.ElapsedTicks >= BreakTick)
                {
                    IsBroken = true;

                    EventTracker.Instance.AddEvent(new VClamBasketBreakEvent()
                    {
                        Team = OwningTeam
                    });

                    UpdateBrokenFrames(600); // default, 10 seconds
                }
            }
            else
            {
                if (LeftBrokenFrames > 0)
                {
                    LeftBrokenFrames--;
                }

                if (RepairTick == 0)
                {
                    return;
                }

                if (args.ElapsedTicks >= RepairTick)
                {
                    Reset();

                    EventTracker.Instance.AddEvent(new VClamBasketRepairEvent()
                    {
                        Team = OwningTeam
                    });
                }
            }
        }

    }
}
