﻿using NintendoNetcode.Pia;
using SharpPcap.LibPcap;

namespace BlitzSniffer.Receiver
{
    // Replays the session as fast as the CPU allows it
    public class ReplayPacketReceiver : PacketReceiver
    {
        public ReplayPacketReceiver(PiaSessionType sessionType, string path) : base(sessionType)
        {
            Device = new CaptureFileReaderDevice(path);
            Device.Open();
        }

        public override void Start(string outputFile = null)
        {
            Device.Filter = "ip and udp";

            base.Start(outputFile);
        }

    }
}
