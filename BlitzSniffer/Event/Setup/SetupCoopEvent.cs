using BlitzSniffer.Event.Setup.Rule;

namespace BlitzSniffer.Event.Setup
{
    class SetupCoopEvent : SetupEvent
    {
        public override string Name => "SetupCoop";

        public SetupCoopEvent() : base()
        {
            RuleConfiguration = new SetupGenericRuleConfiguration();
        }

    }
}
