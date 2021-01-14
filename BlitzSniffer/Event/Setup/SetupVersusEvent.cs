using Blitz.Cmn.Def;
using BlitzCommon.Blitz.Cmn.Def;
using BlitzSniffer.Event.Setup.Player;
using BlitzSniffer.Event.Setup.Rule;
using BlitzSniffer.Resources;
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

        protected override SetupPlayer GetSetupPlayer(uint playerId, Tracker.Player.Player player)
        {
            return new SetupVersusPlayer()
            {
                Id = playerId,
                Name = player.Name,
                Weapon = new SetupWeapon()
                {
                    Id = WeaponResource.Instance.GetMainWeapon((int)player.Weapon.Id),
                    Sub = WeaponResource.Instance.GetSubWeapon((int)player.Weapon.SubId),
                    Special = WeaponResource.Instance.GetSpecialWeapon((int)player.Weapon.SpecialId)
                },
                Headgear = new SetupGear(GearKind.Head, player.Headgear),
                Clothes = new SetupGear(GearKind.Clothes, player.Clothes),
                Shoes = new SetupGear(GearKind.Shoes, player.Shoes)
            };
        }

    }
}
