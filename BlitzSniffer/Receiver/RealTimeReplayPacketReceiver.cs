using NintendoNetcode.Pia;
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

        private PosixTimeval WaitForTimeval
        {
            get;
            set;
        } = null;

        private ManualResetEvent ContinueSignal
        {
            get;
            set;
        }

        private bool IncrementThreadStop = false;

        private object TimevalLock = new object();

        public RealTimeReplayPacketReceiver(PiaSessionType sessionType, string path, int offset) : base(sessionType, path)
        {
            IncrementThread = new Thread(TimeIncrement);
            RealTimeStartOffset = offset;
            ContinueSignal = new ManualResetEvent(false);
        }

        public RealTimeReplayPacketReceiver(PiaSessionType sessionType, string path) : this(sessionType, path, 0)
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
            base.Dispose();

            IncrementThreadStop = true;
            IncrementThread.Join();
            
            ContinueSignal.Dispose();
        }

        // This is probably terrible, but it works
        private void TimeIncrement()
        {
            while (!IncrementThreadStop)
            {
                Thread.Sleep(1);

                lock (TimevalLock)
                {
                    Timeval.MicroSeconds += 1000;
                    if (Timeval.MicroSeconds >= 1000000)
                    {
                        Timeval.Seconds++;
                        Timeval.MicroSeconds = Timeval.MicroSeconds % 1000000;
                    }

                    if (WaitForTimeval != null)
                    {
                        if (WaitForTimeval < Timeval)
                        {
                            ContinueSignal.Set();
                        }
                    }
                }
            }
        }

        protected override void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            PosixTimeval packetTimeval = e.Packet.Timeval;

            bool shouldWait = false;

            if (packetTimeval > Timeval)
            {
                shouldWait = true;

                lock (TimevalLock)
                {
                    WaitForTimeval = packetTimeval;
                }
            }

            if (shouldWait)
            {
                ContinueSignal.WaitOne();

                lock (TimevalLock)
                {
                    WaitForTimeval = null;

                    ContinueSignal.Reset();
                }
            }

            base.OnPacketArrival(sender, e);
        }

    }
}
