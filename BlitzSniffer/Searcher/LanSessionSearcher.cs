using NintendoNetcode.Pia;
using NintendoNetcode.Pia.Lan.Content.Browse;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using Syroot.BinaryData;
using System.IO;

namespace BlitzSniffer.Searcher
{
    class LanSessionSearcher : SessionSearcher
    {
        private ICaptureDevice Device;

        private LanSessionSearcher(ICaptureDevice device) : base()
        {
            Device = device;
            Device.OnPacketArrival += OnPacketArrival;
        }

        public static void Initialize(ICaptureDevice device)
        {
            Instance = new LanSessionSearcher(device);
        }

        protected virtual void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            Packet packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            UdpPacket udpPacket = packet.Extract<UdpPacket>();
            if (udpPacket.DestinationPort != 30000)
            {
                return;
            }

            using (MemoryStream memoryStream = new MemoryStream(udpPacket.PayloadData))
            using (BinaryDataReader reader = new BinaryDataReader(memoryStream))
            {
                byte firstByte = reader.ReadByte();

                if (firstByte == 0x1)
                {
                    LanContentBrowseReply browseReply = new LanContentBrowseReply(reader);
                    byte[] key = PiaEncryptionUtil.GenerateLanSessionKey(browseReply.SessionInfo.SessionParam, PiaEncryptionUtil.BlitzGameKey);

                    NotifySessionFound(key);
                }
            }
        }


    }
}
