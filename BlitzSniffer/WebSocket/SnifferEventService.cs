using BlitzSniffer.Event;
using System;
using System.Text.Json;
using WebSocketSharp.Server;

namespace BlitzSniffer.WebSocket
{
    class SnifferEventService : WebSocketBehavior
    {
        public SnifferEventService()
        {
            EventTracker.Instance.SendEvent += BroadcastEvent;
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            Console.WriteLine("[SnifferEventService] Client connected");
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
