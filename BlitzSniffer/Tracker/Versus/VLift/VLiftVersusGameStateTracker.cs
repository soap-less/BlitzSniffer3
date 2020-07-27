using Blitz.Cmn.Def;
using BlitzSniffer.Clone;
using BlitzSniffer.Event;
using BlitzSniffer.Event.Versus;
using BlitzSniffer.Event.Versus.VLift;
using Nintendo.Sead;
using Syroot.BinaryData;
using System.Collections.Generic;
using System.IO;

namespace BlitzSniffer.Tracker.Versus.VLift
{
    class VLiftVersusGameStateTracker : GachiVersusGameStateTracker
    {
        public override VersusRule Rule => VersusRule.Vlf;

        private VLiftRail Rail;

        public VLiftVersusGameStateTracker(ushort stage, Color4f alpha, Color4f bravo) : base(stage, alpha, bravo)
        {
            Rail = new VLiftRail();

            // Find the VLift rail
            List<dynamic> railPoints = null;
            foreach (Dictionary<string, dynamic> rail in StageLayout["Rails"])
            {
                if (rail["UnitConfigName"] == "Rail_VLift")
                {
                    railPoints = rail["RailPoints"];
                    break;
                }
            }

            if (railPoints == null)
            {
                throw new SnifferException("Cannot find VLift rail");
            }

            // Find the starting rail index
            int startIdx = (railPoints.Count - 1) / 2;

            // Get all the checkpoints
            for (int i = startIdx; i < railPoints.Count; i++)
            {
                Dictionary<string, dynamic> railPoint = railPoints[i];
                uint checkpointHp = (uint)railPoint["CheckPointHP"];

                if (checkpointHp != 0)
                {
                    Rail.AddCheckpoint(checkpointHp);
                }
            }

            CloneHolder.Instance.RegisterClone(121);
            CloneHolder.Instance.CloneChanged += HandleVLiftState;
        }

        public override void Dispose()
        {
            CloneHolder.Instance.CloneChanged -= HandleVLiftState;
        }

        public List<List<uint>> BuildCheckpointListForSetup()
        {
            return new List<List<uint>>()
            {
                Rail.GetCheckpointsForSetup(Team.Alpha),
                Rail.GetCheckpointsForSetup(Team.Bravo)
            };
        }

        private void HandleVLiftState(object sender, CloneChangedEventArgs args)
        {
            if (args.CloneId != 121 || args.ElementId != 4)
            {
                return;
            }

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                reader.Seek(8);

                uint ReadScore()
                {
                    return (uint)(reader.ReadSingle() * 100);
                }

                reader.Seek(8); // skip best VLift position
                uint alphaBestScore = ReadScore();
                uint bravoBestScore = ReadScore();

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

                reader.Seek(8); // skip best total checkpoint HP since we keep track of this internally
                uint alphaCurrentTotalCheckpointHp = (uint)reader.ReadSingle();
                uint bravoCurrentTotalCheckpointHp = (uint)reader.ReadSingle();

                Rail.UpdateCheckpoints(alphaCurrentTotalCheckpointHp, Team.Alpha);
                Rail.UpdateCheckpoints(bravoCurrentTotalCheckpointHp, Team.Bravo);
            }
        }

    }
}
