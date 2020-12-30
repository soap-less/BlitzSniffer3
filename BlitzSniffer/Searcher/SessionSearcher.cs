using Serilog;
using Serilog.Core;
using System;

namespace BlitzSniffer.Searcher
{
    abstract class SessionSearcher : IDisposable
    {
        private static readonly ILogger LogContext = Log.ForContext(Constants.SourceContextPropertyName, "SessionSearcher");

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

            LogContext.Information("Key found: {key}", BitConverter.ToString(key).Replace("-", "").ToLower());
        }

    }
}
