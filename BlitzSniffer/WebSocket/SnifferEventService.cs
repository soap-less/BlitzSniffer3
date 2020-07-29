using BlitzSniffer.Event;
using Serilog;
using Serilog.Core;
using System.Text.Json;
using WebSocketSharp.Server;

namespace BlitzSniffer.WebSocket
{
    class SnifferEventService : WebSocketBehavior
    {
        private static readonly ILogger LogContext = Serilog.Log.ForContext(Constants.SourceContextPropertyName, "SnifferEventService");

        public SnifferEventService()
        {
            EventTracker.Instance.SendEvent += BroadcastEvent;
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            LogContext.Information("Client connected");
        }

        private void BroadcastEvent(object sender, SendEventArgs args)
        {
            string json = JsonSerializer.Serialize(args.GameEvent, args.GameEvent.GetType(), new JsonSerializerOptions()
            {
                WriteIndented = true
            });

            Sessions.BroadcastAsync(json, null);
        }

    }
}
