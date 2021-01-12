using Blitz.Cmn.Def;
using BlitzSniffer.Event.Setup.Rule;
using BlitzSniffer.Tracker;
using BlitzSniffer.Tracker.Versus;

namespace BlitzSniffer.Event.Setup
{
    public class SetupVersusEvent : SetupEvent
    {
        public VersusRule Rule
        {
            get;
            set;
        }

        public SetupVersusEvent()
        {
            VersusGameStateTracker stateTracker = GameSession.Instance.GameStateTracker as VersusGameStateTracker;

            Rule = stateTracker.Rule;
            
            switch (Rule)
            {
                case VersusRule.Var:
                    RuleConfiguration = new SetupVAreaRuleConfiguration();
                    break;
                case VersusRule.Vlf:
                    RuleConfiguration = new SetupVLiftRuleConfiguration();
                    break;
                default:
                    RuleConfiguration = new SetupGenericRuleConfiguration();
                    break;
            }
        }

    }
}
