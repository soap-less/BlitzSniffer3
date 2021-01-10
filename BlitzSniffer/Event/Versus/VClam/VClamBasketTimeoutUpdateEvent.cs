namespace BlitzSniffer.Event.Versus.VClam
{
    class VClamBasketTimeoutUpdateEvent : VClamBasketEvent
    {
        public override string Name => "VClamBasketTimeoutUpdate";

        public uint Timeout
        {
            get;
            set;
        }

    }
}
