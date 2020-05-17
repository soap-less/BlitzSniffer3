using SharpPcap.LibPcap;

namespace BlitzSniffer.Receiver
{
    // Replays the session as fast as the CPU allows it
    public class ReplayPacketReceiver : PacketReceiver
    {
        public ReplayPacketReceiver(string path)
        {
            Device = new CaptureFileReaderDevice(path);
            Device.Open();
        }

    }
}
