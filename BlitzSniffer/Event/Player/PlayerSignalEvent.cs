namespace BlitzSniffer.Event.Player
{
    class PlayerSignalEvent : PlayerEvent
    {
        public override string Name => "PlayerSignalEvent";

        public int SignalType
        {
            get;
            set;
        }

    }
}
