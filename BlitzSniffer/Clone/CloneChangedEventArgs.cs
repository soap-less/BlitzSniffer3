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

        public CloneChangedEventArgs(uint cid, uint eid, byte[] data)
        {
            CloneId = cid;
            ElementId = eid;
            Data = data;
        }

    }
}
