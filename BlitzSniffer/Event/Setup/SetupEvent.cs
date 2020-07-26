using Blitz.Cmn.Def;
using BlitzSniffer.Event.Setup.Rule;
using BlitzSniffer.Tracker;
using BlitzSniffer.Tracker.Versus;
using System.Collections.Generic;
using System.Diagnostics;

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

        // Hack: Serialization of a polymorphic type hierarchy not supported, but can be forced if the type is
        // object. So, the actual SetupRuleConfiguration is stored in a private variable, and a property with
        // type object is exposed to the serializer.
        // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to#serialize-properties-of-derived-classes
        private SetupRuleConfiguration _RuleConfiguration;

        public object RuleConfiguration
        {
            get
            {
                return _RuleConfiguration;
            }
            set
            {
                Trace.Assert(value is SetupRuleConfiguration);

                _RuleConfiguration = value as SetupRuleConfiguration;
            }
        }

        public List<SetupTeam> Teams
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

            GameSession session = GameSession.Instance;
            VersusGameStateTracker stateTracker = session.GameStateTracker as VersusGameStateTracker;

            StageId = stateTracker.StageId;
            Rule = stateTracker.Rule;
            Teams[0].Color = stateTracker.AlphaColor;
            Teams[1].Color = stateTracker.BravoColor;

            switch (Rule)
            {
                case VersusRule.Vlf:
                    RuleConfiguration = new SetupVLiftRuleConfiguration();
                    break;
                default:
                    RuleConfiguration = new SetupGenericRuleConfiguration();
                    break;
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
