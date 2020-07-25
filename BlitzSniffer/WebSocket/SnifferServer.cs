using WebSocketSharp.Server;

namespace BlitzSniffer.WebSocket
{
    public class SnifferServer
    {
        public static SnifferServer Instance = null;

        private WebSocketServer Server
        {
            get;
            set;
        }

        private SnifferServer()
        {
            Server = new WebSocketServer(13370);
            Server.AddWebSocketService<SnifferEventService>("/Events");
            Server.Start();
        }

        public static void Initialize()
        {
            if (Instance != null)
            {
                return;
            }

            Instance = new SnifferServer();
        }

    }
}
