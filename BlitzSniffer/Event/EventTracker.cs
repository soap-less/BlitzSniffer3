using System.Collections.Concurrent;
using System.Threading;

namespace BlitzSniffer.Event
{
    public class EventTracker
    {
        public static readonly EventTracker Instance = new EventTracker();

        private readonly BlockingCollection<GameEvent> EventQueue;
        private readonly CancellationTokenSource Token;

        public delegate void SendEventHandler(object sender, SendEventArgs args);
        public event SendEventHandler SendEvent;

        public EventTracker()
        {
            EventQueue = new BlockingCollection<GameEvent>();
            Token = new CancellationTokenSource();

            new Thread(SendEvents).Start();
        }

        public void AddEvent(GameEvent gameEvent)
        {
            EventQueue.Add(gameEvent);
        }

        public void Shutdown()
        {
            Token.Cancel();
        }

        private void SendEvents()
        {
            while (!Token.IsCancellationRequested)
            {
                if (EventQueue.TryTake(out GameEvent gameEvent))
                {
                    SendEventArgs args = new SendEventArgs(gameEvent);
                    SendEvent?.Invoke(this, args);
                }
            }
        }

    }
}
