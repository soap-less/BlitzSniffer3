using System.Collections.Concurrent;
using System.Threading;

namespace BlitzSniffer.Event
{
    public class EventTracker
    {
        public static readonly EventTracker Instance = new EventTracker();

        private readonly BlockingCollection<GameEvent> EventQueue;

        public delegate void SendEventHandler(object sender, SendEventArgs args);
        public event SendEventHandler SendEvent;

        public EventTracker()
        {
            EventQueue = new BlockingCollection<GameEvent>();

            new Thread(SendEvents).Start();
        }

        public void AddEvent(GameEvent gameEvent)
        {
            EventQueue.Add(gameEvent);
        }

        private void SendEvents()
        {
            while (true)
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
