using System;

namespace BlitzSniffer.Searcher
{
    public class SessionFoundArgs : EventArgs
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

        public SessionFoundArgs(SessionFoundDataType type, byte[] data)
        {
            FoundDataType = type;
            Data = data;
        }

    }
}
