using Blitz.Cmn.Def;
using Blitz.Sead;
using BlitzSniffer.Clone;
using BlitzSniffer.Event;
using BlitzSniffer.Event.Setup;
using BlitzSniffer.Tracker.Player;
using BlitzSniffer.Tracker.Versus;
using Syroot.BinaryData;
using System;
using System.IO;

namespace BlitzSniffer.Tracker
{
    class GameSession : IDisposable
    {
        public static GameSession _currentSession = null;

        public static GameSession CurrentSession
        {
            get
            {
                if (_currentSession == null)
                {
                    _currentSession = new GameSession();
                }

                return _currentSession;
            }
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

        public GameSession()
        {
            PlayerTracker = new PlayerTracker();
            GameStateTracker = null;

            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged += HandleSeqEventVersusSetting;

            holder.RegisterClone(2);
        }

        public void Dispose()
        {
            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged -= HandleSeqEventVersusSetting;

            PlayerTracker.Dispose();

            if (GameStateTracker != null)
            {
                GameStateTracker.Dispose();
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
                ushort rule = reader.ReadUInt16();

                reader.Seek(14, SeekOrigin.Begin);
                uint teamBits = reader.ReadUInt32();

                PlayerTracker.SetTeams(teamBits);

                // TODO verify these
                reader.Seek(52, SeekOrigin.Begin);
                Color4f alpha = new Color4f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0);

                reader.Seek(72, SeekOrigin.Begin);
                Color4f bravo = new Color4f(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), 0);

                // TODO ranked
                GameStateTracker = new GenericVersusGameStateTracker(stage, rule, alpha, bravo);

                EventTracker.Instance.AddEvent(new SetupEvent());

                return;
            }
        }

        public static void Initialize()
        {
            if (_currentSession == null)
            {
                _currentSession = new GameSession();
            }
        }

        public static void ResetSession()
        {
            _currentSession.Dispose();
            _currentSession = new GameSession();
        }

    }
}
