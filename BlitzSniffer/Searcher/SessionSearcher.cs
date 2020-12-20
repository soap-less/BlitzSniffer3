namespace BlitzSniffer.Searcher
{
    abstract class SessionSearcher
    {
        public static SessionSearcher Instance = null;

        public delegate void SessionFoundHandler(object sender, SessionFoundArgs args);
        public event SessionFoundHandler SessionFound;

        protected SessionSearcher()
        {

        }

        protected void NotifySessionFound(byte[] key)
        {
            SessionFound?.Invoke(this, new SessionFoundArgs(key));
        }

    }
}
