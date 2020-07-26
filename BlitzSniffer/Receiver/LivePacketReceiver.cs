using SharpPcap;
using SharpPcap.LibPcap;
using SharpPcap.Npcap;

namespace BlitzSniffer.Receiver
{
    public class LivePacketReceiver : PacketReceiver
    {
        private static readonly int ReadTimeout = 1;

        public LivePacketReceiver(ICaptureDevice device)
        {
            Device = device;

            NpcapDevice npcapDevice = device as NpcapDevice;
            if (npcapDevice != null)
            {
                npcapDevice.Open(OpenFlags.DataTransferUdp, ReadTimeout);
            }
            else
            {
                LibPcapLiveDevice libPcapDevice = device as LibPcapLiveDevice;
                device.Open(DeviceMode.Promiscuous, ReadTimeout);
            }
        }

    }
}
