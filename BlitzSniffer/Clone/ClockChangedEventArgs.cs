using System;

namespace BlitzSniffer.Clone
{
    public class ClockChangedEventArgs : EventArgs
    {
        public uint Clock
        {
            get;
            set;
        }

        public ClockChangedEventArgs(uint clock)
        {
            Clock = clock;
        }

    }
}
