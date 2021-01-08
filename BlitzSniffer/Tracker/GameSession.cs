using Blitz.Cmn.Def;
using BlitzSniffer.Clone;
using BlitzSniffer.Event;
using BlitzSniffer.Event.Setup;
using BlitzSniffer.Tracker.Player;
using BlitzSniffer.Tracker.Station;
using BlitzSniffer.Tracker.Versus;
using BlitzSniffer.Tracker.Versus.VArea;
using BlitzSniffer.Tracker.Versus.VGoal;
using BlitzSniffer.Tracker.Versus.VLift;
using Nintendo.Sead;
using Syroot.BinaryData;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BlitzSniffer.Tracker
{
    class GameSession
    {
        // Increment this if additional synchronization is needed before SetupEvent can fire!
        private static readonly int SETUP_NECESSARY_SIGNALS = 2;

        public static GameSession Instance
        {
            get;
            private set;
        }

        public StationTracker StationTracker
        {
            get;
            private set;
        }

        public PlayerTracker PlayerTracker
        {
            get;
            private set;
        }

        public GameStateTracker GameStateTracker
        {
            get;
            private set;
        }

        public uint ElapsedTicks
        {
            get;
            set;
        }

        private Task SetupEventTask
        {
            get;
            set;
        }

        private CancellationTokenSource SetupEventCts
        {
            get;
            set;
        }

        private CountdownEvent SetupEventCountdown
        {
            get;
            set;
        }

        public bool IsSetup
        {
            get;
            private set;
        }

        private bool ClockReady;
        private uint StartClock;
        private uint CurrentClock;

        public delegate void GameTickedHandler(object sender, GameTickedEventArgs args);
        public event GameTickedHandler GameTicked;

        private GameSession()
        {
            PlayerTracker = null;
            StationTracker = new StationTracker(); 
            GameStateTracker = null;

            SetupEventTask = null;
            SetupEventCts = new CancellationTokenSource();
            SetupEventCountdown = new CountdownEvent(SETUP_NECESSARY_SIGNALS);

            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged += HandleSeqEventVersusSetting;
            holder.CloneChanged += HandleSystemEvent;

            holder.ClockChanged += HandleClockChanged;

            holder.RegisterClone(2);
            holder.RegisterClone(100);
        }

        public static void Initialize()
        {
            Instance = new GameSession();
        }

        public void Reset()
        {
            if (SetupEventTask != null && SetupEventTask.Status == TaskStatus.Running)
            {
                SetupEventCts.Cancel();

                SetupEventTask.Wait();
            }

            SetupEventCountdown.Reset();

            if (PlayerTracker != null)
            {
                PlayerTracker.Dispose();
            }
            
            PlayerTracker = new PlayerTracker();

            if (GameStateTracker != null)
            {
                GameStateTracker.Dispose();
                GameStateTracker = null;
            }

            if (IsSetup)
            {
                EventTracker.Instance.AddEvent(new SessionResetEvent());
            }

            IsSetup = false;
            ElapsedTicks = 0;
            ClockReady = false;
            StartClock = 0;
            CurrentClock = 0;

            SetupEventTask = Task.Run(FireSetupEvent);
        }

        public void SignalSetupReady()
        {
            SetupEventCountdown.Signal();
        }

        private void FireSetupEvent()
        {
            try
            {
                SetupEventCountdown.Wait(SetupEventCts.Token);

                IsSetup = true;

                EventTracker.Instance.AddEvent(new SetupEvent());
            }
            catch (OperationCanceledException)
            {
                ;
            }
        }

        private void HandleSeqEventVersusSetting(object sender, CloneChangedEventArgs args)
        {
            if (args.CloneId != 2 || args.ElementId != 5)
            {
                return;
            }

            if (GameStateTracker != null)
            {
                return;
            }

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                reader.Seek(2);
                ushort stage = reader.ReadUInt16();
                VersusRule rule = (VersusRule)reader.ReadUInt16();

                reader.Seek(14, SeekOrigin.Begin);
                uint teamBits = reader.ReadUInt32();

                PlayerTracker.SetTeamBits(teamBits);

                // TODO verify these
                reader.Seek(52, SeekOrigin.Begin);
                Color4f alpha = new Color4f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 1.0f);

                reader.Seek(72, SeekOrigin.Begin);
                Color4f bravo = new Color4f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 1.0f);

                switch (rule)
                {
                    case VersusRule.Vgl:
                        GameStateTracker = new VGoalVersusGameStateTracker(stage, alpha, bravo);
                        break;
                    case VersusRule.Var:
                        GameStateTracker = new VAreaVersusGameStateTracker(stage, alpha, bravo);
                        break;
                    case VersusRule.Vlf:
                        GameStateTracker = new VLiftVersusGameStateTracker(stage, alpha, bravo);
                        break;
                    default:
                        GameStateTracker = new GenericVersusGameStateTracker(stage, rule, alpha, bravo);
                        break;
                }
            }
        }

        private void HandleSystemEvent(object sender, CloneChangedEventArgs args)
        {
            if (args.CloneId != 100 || args.ElementId != 1)
            {
                return;
            }

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                uint eventType = reader.ReadUInt32();

                if (eventType == 0) // Synchronize game start using clone clock
                {
                    StartClock = reader.ReadUInt32();
                    // int mapId = reader.ReadInt32();
                    // uint unknown = reader.ReadUInt32();

                    ClockReady = true;
                }
            }
        }

        public void HandleClockChanged(object sender, ClockChangedEventArgs args)
        {
            if (!ClockReady)
            {
                return;
            }

            // Only tick if we've passed the start
            if (args.Clock >= StartClock)
            {
                uint elapsedClockTicks = args.Clock - CurrentClock;

                // This is a for loop because we might need to catch up by several ticks.
                // If the clone clock is uneven, CloneClockToGameFrame always rounds down the
                // number of ticks.
                for (uint i = 0; i < Blitz.Lp.Utl.CloneClockToGameFrame(elapsedClockTicks); i++)
                {
                    ElapsedTicks++;

                    GameTicked(this, new GameTickedEventArgs(ElapsedTicks));
                }
            }

            CurrentClock = args.Clock;
        }

    }
}
