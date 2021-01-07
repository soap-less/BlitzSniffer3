using BlitzSniffer.Clone;
using BlitzSniffer.Enl;
using NintendoNetcode.Enl.Record;
using Syroot.BinaryData;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BlitzSniffer.Tracker.Station
{
    class StationTracker
    {
        private readonly Dictionary<ulong, Station> Stations;

        private byte LastJointSeqState
        {
            get;
            set;
        }

        public int ActualConnectedStations
        {
            get;
            private set;
        }

        public StationTracker()
        {
            Stations = new Dictionary<ulong, Station>();
            LastJointSeqState = 0;
            ActualConnectedStations = 0;

            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged += HandleStationInfo;
            holder.CloneChanged += HandlePlayerName;

            for (uint i = 0; i < 10; i++)
            {
                holder.RegisterClone(i + 3);
            }

             EnlHolder.Instance.SystemInfoReceived += HandleEnlSystemInfo;
        }

        public Station GetStationForSsid(ulong sourceId)
        {
            if (Stations.TryGetValue(sourceId, out Station station))
            {
                return station;
            }
            else
            {
                Station newStation = new Station(sourceId);

                Stations.Add(sourceId, newStation);

                return newStation;
            }
        }

        private void HandleSeqStateAllSame(byte seqState)
        {
            if (seqState == LastJointSeqState)
            {
                return;
            }

            switch (seqState)
            {
                case 7: // Apparently this is "ready for game start" - Game::OnlineStartGameExe::stateWaitForSeqState()
                    GameSession.Instance.PlayerTracker.ApplyTeamBits();

                    // SeqState 7 isn't fired until the game is ready to proceed into synchronizing the clocks
                    // and waiting for the game to start (VS intro demo)
                    GameSession.Instance.SignalSetupReady();

                    break;
                case 12: // Results screen start - Game::SeqVersusResult::stateEnterStartResult()
                    GameSession.Instance.Reset();

                    break;
                default:
                    break;
            }

            LastJointSeqState = seqState;
        }

        private void HandleEnlSystemInfo(object sender, SystemInfoReceivedEventArgs args)
        {
            EnlSystemInfoRecord record = args.Record;

            IEnumerable<ulong> disconnectedStations = Stations.Keys.Except(record.Unknown3.Select(u => u.StationId)).ToList();

            foreach (ulong stationId in disconnectedStations)
            {
                Stations.Remove(stationId);
            }
        }

        private void HandleStationInfo(object sender, CloneChangedEventArgs args)
        {
            uint enlId = args.CloneId - 3;
            if (enlId >= 10)
            {
                return;
            }

            if (args.ElementId != 0)
            {
                return;
            }

            Station station = GetStationForSsid(args.SourceStationId);

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                reader.Seek(2); // Bitflag
                byte seqState = reader.ReadByte();
                ActualConnectedStations = reader.ReadByte();

                station.SeqState = seqState;

                if (ActualConnectedStations == Stations.Count)
                {
                    if (Stations.Values.All(s => s.SeqState == seqState))
                    {
                        HandleSeqStateAllSame(seqState);
                    }
                }
            }
                
        }

        private void HandlePlayerName(object sender, CloneChangedEventArgs args)
        {
            uint enlId = args.CloneId - 3;
            if (enlId >= 10)
            {
                return;
            }

            if (args.ElementId != 1)
            {
                return;
            }

            Station station = GetStationForSsid(args.SourceStationId);

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                string name = reader.ReadString(BinaryStringFormat.ZeroTerminated, Encoding.Unicode);
                station.Name = name;

                station.IsSetup = true;
            }
        }

    }
}
