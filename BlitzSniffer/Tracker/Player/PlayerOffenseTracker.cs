using BlitzSniffer.Event;
using BlitzSniffer.Event.Player;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlitzSniffer.Tracker.Player
{
    public class PlayerOffenseTracker
    {
        private static readonly ILogger LogContext = Log.ForContext(Constants.SourceContextPropertyName, "PlayerOffenseTracker");
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
                            LogContext.Information("Removing incomplete event for victim {VictimIdx}", victimIdx);
                        }
                    }
                    else
                    {
                        LogContext.Error("Failed to remove death event for victim {VictimIdx}", victimIdx);
                    }
                });

                return deathEvent;
            }
        }

    }
}
