using Serilog;
using Serilog.Core;

namespace BlitzSniffer.Util
{
    static class LogUtil
    {
        public static ILogger GetLogger(string name)
        {
            return Log.ForContext(Constants.SourceContextPropertyName, name);
        }

    }
}
