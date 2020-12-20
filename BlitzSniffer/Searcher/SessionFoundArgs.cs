using System;

namespace BlitzSniffer.Searcher
{
    public class SessionFoundArgs : EventArgs
    {
        public byte[] SessionKey
        {
            get;
            set;
        }

        public SessionFoundArgs(byte[] key)
        {
            SessionKey = key;
        }

    }
}
