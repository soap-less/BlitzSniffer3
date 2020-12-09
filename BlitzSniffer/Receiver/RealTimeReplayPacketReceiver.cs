using SharpPcap;
using System.Threading;

namespace BlitzSniffer.Receiver
{
    // Replays the session in real-time
    class RealTimeReplayPacketReceiver : ReplayPacketReceiver
    {
        private Thread IncrementThread
        {
            get;
            set;
        }

        private int RealTimeStartOffset
        {
            get;
            set;
        }

        private PosixTimeval Timeval
        {
            get;
            set;
        } = null;

        private PosixTimeval RealTimeStartTimeval
        {
            get;
            set;
        }

        private bool IncrementThreadStop = false;

        private object TimevalLock = new object();

        public RealTimeReplayPacketReceiver(string path, int offset) : base(path)
        {
            IncrementThread = new Thread(TimeIncrement);
            RealTimeStartOffset = offset;
        }

        public RealTimeReplayPacketReceiver(string path) : this(path, 0)
        {

        }

        public override void Start(string outputFile = null)
        {
            lock (TimevalLock)
            {
                RawCapture capture = Device.GetNextPacket();
                Timeval = capture.Timeval;

                // May take a few moments to catch up, but it'll get there eventually
                Timeval.Seconds += (ulong)RealTimeStartOffset;
            }

            IncrementThread.Start();

            base.Start(outputFile);
        }

        public override void Dispose()
        {
            IncrementThreadStop = true;
            base.Dispose();
        }

        // This is probably terrible, but it works
        private void TimeIncrement()
        {
            while (!IncrementThreadStop)
            {
                Thread.Sleep(8);

                lock (TimevalLock)
                {
                    Timeval.MicroSeconds += 8000;
                    if (Timeval.MicroSeconds >= 1000000)
                    {
                        Timeval.Seconds++;
                        Timeval.MicroSeconds = Timeval.MicroSeconds % 1000000;
                    }
                }
            }
        }

        protected override void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            do
            {
                lock (TimevalLock)
                {
                    if (e.Packet.Timeval <= Timeval)
                    {
                        break;
                    }
                }
            }
            while (true);

            base.OnPacketArrival(sender, e);
        }

    }
}
