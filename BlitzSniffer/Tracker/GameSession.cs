using Blitz.Cmn.Def;
using BlitzSniffer.Clone;
using BlitzSniffer.Event;
using BlitzSniffer.Event.Setup;
using BlitzSniffer.Tracker.Player;
using BlitzSniffer.Tracker.Station;
using BlitzSniffer.Tracker.Versus;
using BlitzSniffer.Tracker.Versus.VArea;
using BlitzSniffer.Tracker.Versus.VLift;
using Nintendo.Sead;
using Syroot.BinaryData;
using System.IO;

namespace BlitzSniffer.Tracker
{
    class GameSession
    {
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

        public bool IsSetup
        {
            get;
            private set;
        }

        private GameSession()
        {
            PlayerTracker = new PlayerTracker();
            StationTracker = new StationTracker(); 
            GameStateTracker = null;

            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged += HandleSeqEventVersusSetting;

            holder.RegisterClone(2);
        }

        public static void Initialize()
        {
            Instance = new GameSession();
        }

        public void Reset()
        {
            if (!IsSetup)
            {
                return;
            }

            PlayerTracker.Dispose();
            PlayerTracker = new PlayerTracker();

            if (GameStateTracker != null)
            {
                GameStateTracker.Dispose();
                GameStateTracker = null;
            }

            IsSetup = false;

            EventTracker.Instance.AddEvent(new SessionResetEvent());
        }

        public void FireSetupEvent()
        {
            if (IsSetup)
            {
                return;
            }

            IsSetup = true;

            EventTracker.Instance.AddEvent(new SetupEvent());
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
                Color4f alpha = new Color4f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0);

                reader.Seek(72, SeekOrigin.Begin);
                Color4f bravo = new Color4f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0);

                switch (rule)
                {
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

    }
}
