namespace BlitzSniffer.Event.Player
{
    class PlayerCoopRescuedEvent : PlayerEvent
    {
        public override string Name => "PlayerCoopRescued";

        public int SaviourIdx
        {
            get;
            set;
        }

    }
}
