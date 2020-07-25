using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BlitzSniffer.Tracker.Station
{
    class Station
    {
        public uint EnlId
        {
            get;
            private set;
        }

        public uint PlayerId
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

        public Station(uint enlId)
        {
            EnlId = enlId;
            Reset();
        }

        public void Reset()
        {
            PlayerId = 0xFF; // disconnected
            Name = $"Station {EnlId}";
            SeqState = 0;
        }

    }
}
