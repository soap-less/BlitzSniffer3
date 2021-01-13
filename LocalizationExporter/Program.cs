using Serilog;
using Serilog.Core;

namespace LocalizationExporter
{
    class Program
    {
        private static readonly string LOG_FORMAT = "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: LOG_FORMAT)
                .CreateLogger();

            ILogger logContext = Log.ForContext(Constants.SourceContextPropertyName, "Program");

            logContext.Information("LocalizationExporter {Version}", ThisAssembly.AssemblyFileVersion);
        }

    }
}
