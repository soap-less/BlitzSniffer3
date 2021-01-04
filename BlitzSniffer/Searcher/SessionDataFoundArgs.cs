using System;

namespace BlitzSniffer.Searcher
{
    public class SessionDataFoundArgs : EventArgs
    {
        public SessionFoundDataType FoundDataType
        {
            get;
            set;
        }

        public byte[] Data
        {
            get;
            set;
        }

        public SessionDataFoundArgs(SessionFoundDataType type, byte[] data)
        {
            FoundDataType = type;
            Data = data;
        }

    }
}
