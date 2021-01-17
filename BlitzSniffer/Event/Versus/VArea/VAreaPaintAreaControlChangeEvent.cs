using Blitz.Cmn.Def;

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

        public Team Team
        {
            get;
            set;
        }

    }
}
