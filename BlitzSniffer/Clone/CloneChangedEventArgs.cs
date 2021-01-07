using System;

namespace BlitzSniffer.Clone
{
    public class CloneChangedEventArgs : EventArgs
    {
        public uint CloneId
        {
            get;
            set;
        }

        public uint ElementId
        {
            get;
            set;
        }

        public byte[] Data
        {
            get;
            set;
        }

        public ulong SourceStationId
        {
            get;
            set;
        }

        public CloneChangedEventArgs(uint cid, uint eid, byte[] data, ulong ssid)
        {
            CloneId = cid;
            ElementId = eid;
            Data = data;
            SourceStationId = ssid;
        }

    }
}
