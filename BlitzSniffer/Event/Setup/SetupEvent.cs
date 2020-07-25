using Blitz.Cmn.Def;
using BlitzSniffer.Tracker;
using BlitzSniffer.Tracker.Versus;
using BlitzSniffer.Tracker.Versus.VLift;
using System.Collections.Generic;

namespace BlitzSniffer.Event.Setup
{
    public class SetupEvent : GameEvent
    {
        public override string Name => "Setup";

        public uint StageId
        {
            get;
            set;
        }

        public VersusRule Rule
        {
            get;
            set;
        }

        public List<SetupTeam> Teams
        {
            get;
            set;
        }

        public List<List<uint>> VLiftCheckpoints
        {
            get;
            set;
        }

        public SetupEvent()
        {
            Teams = new List<SetupTeam>();
            for (int i = 0; i < 2; i++)
            {
                Teams.Add(new SetupTeam());
            }

            GameSession session = GameSession.CurrentSession;
            VersusGameStateTracker stateTracker = session.GameStateTracker as VersusGameStateTracker;

            StageId = stateTracker.StageId;
            Rule = stateTracker.Rule;
            Teams[0].Color = stateTracker.AlphaColor;
            Teams[1].Color = stateTracker.BravoColor;

            if (Rule == VersusRule.Vlf)
            {
                VLiftCheckpoints = (session.GameStateTracker as VLiftVersusGameStateTracker).BuildCheckpointListForSetup();
            }
            else
            {
                VLiftCheckpoints = new List<List<uint>>()
                {
                    new List<uint>(),
                    new List<uint>()
                };
            }

            for (uint i = 0; i != 10; i++)
            {
                Tracker.Player.Player trackedPlayer = session.PlayerTracker.GetPlayer(i);

                if (!trackedPlayer.IsActive || trackedPlayer.Team == Team.Neutral)
                {
                    continue;
                }

                SetupPlayer setupPlayer = new SetupPlayer()
                {
                    Id = i,
                    Name = trackedPlayer.Name
                };

                if (trackedPlayer.Team == Team.Alpha)
                {
                    Teams[0].Players.Add(setupPlayer);
                }
                else
                {
                    Teams[1].Players.Add(setupPlayer);
                }
            }
        }

    }
}
