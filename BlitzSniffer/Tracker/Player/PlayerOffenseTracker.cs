using BlitzSniffer.Event;
using BlitzSniffer.Event.Player;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlitzSniffer.Tracker.Player
{
    public class PlayerOffenseTracker
    {
        private static readonly int FRAME_DELAY = 3;

        private readonly ConcurrentDictionary<uint, PlayerDeathEvent> WaitingDeathEvents;

        public PlayerOffenseTracker()
        {
            WaitingDeathEvents = new ConcurrentDictionary<uint, PlayerDeathEvent>();
        }

        public PlayerDeathEvent GetDeathEventForVictim(uint victimIdx)
        {
            if (WaitingDeathEvents.ContainsKey(victimIdx))
            {
                return WaitingDeathEvents[victimIdx];
            }
            else
            {
                PlayerDeathEvent deathEvent = new PlayerDeathEvent()
                {
                    PlayerIdx = victimIdx
                };

                WaitingDeathEvents[victimIdx] = deathEvent;

                Task.Delay(17 * FRAME_DELAY).ContinueWith(x =>
                {
                    if (WaitingDeathEvents.Remove(victimIdx, out _))
                    {
                        if (deathEvent.IsComplete)
                        {
                            EventTracker.Instance.AddEvent(deathEvent);
                        }
                        else
                        {
                            Console.WriteLine($"[PlayerOffenseTracker] Removing incomplete event for victim {victimIdx}");
                        }
                    }
                    else
                    {
                        throw new SnifferException("Failed to remove death event from waiting dict");
                    }
                });

                return deathEvent;
            }
        }

    }
}
