using Blitz.Cmn.Def;
using BlitzSniffer.Event;
using BlitzSniffer.Event.Versus.VClam;
using System;
using System.Diagnostics;

namespace BlitzSniffer.Tracker.Versus.VClam
{
    class VClamBasket : IDisposable
    {
        public Team OwningTeam
        {
            get;
            private set;
        }

        public VClamBasket OppositeBasket
        {
            get
            {
                return _OppositeBasket;
            }
            set
            {
                Trace.Assert(_OppositeBasket == null);

                _OppositeBasket = value;
            }
        }

        public VClamBasketState State
        {
            get;
            private set;
        }

        private VClamBasket _OppositeBasket;
        private uint TargetTick; // variable reused between states
        private uint LeftBrokenFrames;

        public VClamBasket(Team team)
        {
            OwningTeam = team;

            TargetTick = 0;
            LeftBrokenFrames = 0;

            GameSession.Instance.GameTicked += HandleGameTick;
        }

        public void Dispose()
        {
            OppositeBasket = null; // just in case

            GameSession.Instance.GameTicked -= HandleGameTick;
        }

        public void RequestBreak(uint tick)
        {
            Trace.Assert(State == VClamBasketState.Idle);

            TargetTick = tick;
        }

        public uint GetCurrentBreakRequestTick()
        {
            Trace.Assert(State == VClamBasketState.Idle);

            return TargetTick;
        }

        public void NullifyBreakRequest()
        {
            Trace.Assert(State == VClamBasketState.Idle);

            TargetTick = 0;
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
            Trace.Assert(State == VClamBasketState.Broken || State == VClamBasketState.Closed);

            TargetTick = tick;
        }

        private void HandleGameTick(object sender, GameTickedEventArgs args)
        {
            switch (State)
            {
                case VClamBasketState.Idle:
                    if (TargetTick == 0)
                    {
                        return;
                    }

                    if (args.ElapsedTicks >= TargetTick)
                    {
                        TargetTick = 0;

                        EventTracker.Instance.AddEvent(new VClamBasketBreakEvent()
                        {
                            Team = OwningTeam
                        });

                        UpdateBrokenFrames(600); // default, 10 seconds

                        State = VClamBasketState.Broken;
                    }

                    break;
                case VClamBasketState.Broken:
                    if (LeftBrokenFrames > 0)
                    {
                        LeftBrokenFrames--;

                        if (LeftBrokenFrames == 0)
                        {
                            State = VClamBasketState.Closed;
                        }
                    }

                    break;
                case VClamBasketState.Closed:
                    if (TargetTick == 0)
                    {
                        return;
                    }

                    if (args.ElapsedTicks >= TargetTick)
                    {
                        TargetTick = 0;

                        EventTracker.Instance.AddEvent(new VClamBasketRepairEvent()
                        {
                            Team = OwningTeam
                        });

                        State = VClamBasketState.Idle;
                    }

                    break;
            }
        }

    }
}
