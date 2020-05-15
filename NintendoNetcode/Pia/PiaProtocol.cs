namespace NintendoNetcode.Pia
{
    public enum PiaProtocol : byte
    {
        KeepAlive = 0x08,
        Station = 0x14,
        Mesh = 0x18,
        SyncClock = 0x1c,
        Ldn = 0x24,
        Direct = 0x28,
        Net = 0x2c,
        NatTraversal = 0x34,
        BandwidthChecker = 0x54,
        Rtt = 0x58,
        Sync = 0x65,
        Unreliable = 0x68,
        RoundRobinUnreliable = 0x6c,
        Clone = 0x74,
        Voice = 0x78,
        Reliable = 0x7c,
        Lan = 0x44,
        BroadcastReliable = 0x80,
        StreamBroadcastReliable = 0x81,
        ReliableBroadcast = 0x84,
        Session = 0x94,
        Lobby = 0x98,
        MonitoringData = 0xa4,
        RelayService = 0xa8,
        WanNat = 0xac
    }
}
