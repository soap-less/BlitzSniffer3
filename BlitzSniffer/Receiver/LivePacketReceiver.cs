using SharpPcap.LibPcap;

namespace BlitzSniffer.Receiver
{
    public class LivePacketReceiver : PacketReceiver
    {
        public LivePacketReceiver(PcapDevice device)
        {
            Device = device;
            device.Open();
        }

    }
}
