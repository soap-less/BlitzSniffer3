using System;

namespace BlitzSniffer.Searcher
{
    abstract class SessionSearcher : IDisposable
    {
        public static SessionSearcher Instance = null;

        public delegate void SessionFoundHandler(object sender, SessionFoundArgs args);
        public event SessionFoundHandler SessionFound;

        protected SessionSearcher()
        {

        }

        public abstract void Dispose();

        protected void NotifySessionFound(byte[] key)
        {
            SessionFound?.Invoke(this, new SessionFoundArgs(key));
        }

    }
}
