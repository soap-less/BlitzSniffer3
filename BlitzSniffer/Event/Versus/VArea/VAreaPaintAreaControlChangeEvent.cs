namespace BlitzSniffer.Event.Versus.VArea
{
    class VAreaPaintAreaControlChangeEvent : GameEvent
    {
        public override string Name => "VAreaPaintAreaControlChange";

        public int AreaIdx
        {
            get;
            set;
        }

        public uint Team
        {
            get;
            set;
        }

    }
}
