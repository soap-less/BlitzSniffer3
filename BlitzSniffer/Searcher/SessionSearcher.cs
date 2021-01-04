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

        protected void NotifySessionDataFound(SessionFoundDataType type, byte[] data)
        {
            SessionFound?.Invoke(this, new SessionFoundArgs(type, data));

            if (type == SessionFoundDataType.Key)
            {
                LogContext.Information("Key found: {Key}", BitConverter.ToString(data).Replace("-", "").ToLower());
            }
            else if (type == SessionFoundDataType.GatheringId)
            {
                LogContext.Information("Gathering ID found: {GatheringId}", BitConverter.ToString(data).Replace("-", "").ToLower());
            }
        }

    }
}
