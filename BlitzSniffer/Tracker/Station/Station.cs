namespace BlitzSniffer.Tracker.Station
{
    class Station
    {
        private string DefaultName
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        // Cnet::Def::SeqState
        public uint SeqState
        {
            get;
            set;
        }

        // Cnet::PacketPlayerInfo
        public byte[] PlayerInfo
        {
            get;
            set;
        }

        public bool IsSetup
        {
            get;
            set;
        }

        public Station(ulong ssid)
        {
            DefaultName = $"{ssid:x16}";
            Reset();
        }

        public void Reset()
        {
            Name = DefaultName;
            SeqState = 0;
            IsSetup = false;
        }

    }
}
