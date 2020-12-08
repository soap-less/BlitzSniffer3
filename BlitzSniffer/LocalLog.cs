using BlitzSniffer.Event;
using BlitzSniffer.Event.Player;
using BlitzSniffer.Event.Versus;
using BlitzSniffer.Event.Versus.VArea;
using BlitzSniffer.Event.Versus.VLift;
using BlitzSniffer.Tracker;
using BlitzSniffer.Tracker.Player;
using Serilog;
using Serilog.Core;
using System.Text.Json;

namespace BlitzSniffer
{
    class LocalLog
    {
        private static readonly ILogger LogContext = Log.ForContext(Constants.SourceContextPropertyName, "LocalLog");

        public static void RegisterConsoleDebug()
        {
            EventTracker.Instance.SendEvent += PrintEventToConsole;
        }

        static void PrintEventToConsole(object sender, SendEventArgs args)
        {
            if (args.GameEvent is PlayerDeathEvent)
            {
                PlayerDeathEvent deathEvent = args.GameEvent as PlayerDeathEvent;
                PlayerTracker tracker = GameSession.Instance.PlayerTracker;
                Player victimPlayer = tracker.GetPlayer(deathEvent.PlayerIdx);
                Player attackerPlayer = deathEvent.AttackerIdx != -1 ? tracker.GetPlayer((uint)deathEvent.AttackerIdx) : null;
                Player assisterPlayer = deathEvent.AssisterIdx != -1 ? tracker.GetPlayer((uint)deathEvent.AssisterIdx) : null;

                string deathStr;
                if (attackerPlayer != null)
                {
                    deathStr = $"{victimPlayer.Name} was killed by {attackerPlayer.Name} using {deathEvent.Cause}";
                }
                else
                {
                    deathStr = $"{victimPlayer.Name} died by {deathEvent.Cause}";
                }

                if (assisterPlayer != null)
                {
                    deathStr += $" with help from {assisterPlayer.Name}";
                }

                LogContext.Information("PlayerDeath: {DeathString}", deathStr);
            }
            else if (args.GameEvent is PlayerRespawnEvent)
            {
                PlayerRespawnEvent respawnEvent = args.GameEvent as PlayerRespawnEvent;
                LogContext.Information("PlayerRespawn: {Name} respawned", GameSession.Instance.PlayerTracker.GetPlayer(respawnEvent.PlayerIdx).Name);
            }
            else if (args.GameEvent is PlayerSpecialActivateEvent)
            {
                PlayerSpecialActivateEvent specialEvent = args.GameEvent as PlayerSpecialActivateEvent;
                LogContext.Information("PlayerSpecialActivate: {Name} activated their special weapon.", GameSession.Instance.PlayerTracker.GetPlayer(specialEvent.PlayerIdx).Name);
            }
            else if (args.GameEvent is GachiScoreUpdateEvent)
            {
                GachiScoreUpdateEvent scoreUpdateEvent = args.GameEvent as GachiScoreUpdateEvent;
                LogContext.Information("GachiScoreUpdate: alpha {AlphaScore} + {AlphaPenalty}, bravo {BravoScore} + {BravoPenalty}", scoreUpdateEvent.AlphaScore, scoreUpdateEvent.AlphaPenalty, scoreUpdateEvent.BravoScore, scoreUpdateEvent.BravoPenalty);
            }
            else if (args.GameEvent is GachiOvertimeStartEvent)
            {
                LogContext.Information("GachiOvertimeStart: overtime is starting now");
            }
            else if (args.GameEvent is GachiOvertimeTimeoutUpdateEvent)
            {
                GachiOvertimeTimeoutUpdateEvent overtimeTimeoutEvent = args.GameEvent as GachiOvertimeTimeoutUpdateEvent;
                LogContext.Information("GachiOvertimeTimeoutUpdate: overtime will end in {Ticks} ticks", overtimeTimeoutEvent.Length);
            }
            else if (args.GameEvent is GachiFinishEvent)
            {
                GachiFinishEvent finishEvent = args.GameEvent as GachiFinishEvent;
                LogContext.Information("GachiFinishEvent: game finish, {AlphaScore} - {BravoScore}", finishEvent.AlphaScore, finishEvent.BravoScore);
            }
            /*else if (args.GameEvent is VLiftPositionUpdateEvent)
            {
                VLiftPositionUpdateEvent positionEvent = args.GameEvent as VLiftPositionUpdateEvent;
                LogContext.Information("VLiftPositionUpdate: alpha {AlphaPosition}%, bravo {BravoPosition}%", positionEvent.AlphaPosition, positionEvent.BravoPosition);
            }*/
            else if (args.GameEvent is VLiftCheckpointUpdateEvent)
            {
                VLiftCheckpointUpdateEvent checkpointUpdateEvent = args.GameEvent as VLiftCheckpointUpdateEvent;
                LogContext.Information("VLiftCheckpointUpdate: team {Team}, checkpoint {Index}, HP {Hp}, best HP {BestHp}", checkpointUpdateEvent.Team, checkpointUpdateEvent.Idx, checkpointUpdateEvent.Hp, checkpointUpdateEvent.BestHp);
            }
            else if (args.GameEvent is VAreaPaintAreaCappedStateUpdateEvent)
            {
                VAreaPaintAreaCappedStateUpdateEvent cappedEvent = args.GameEvent as VAreaPaintAreaCappedStateUpdateEvent;
                LogContext.Information("VAreaPaintAreaCappedStateUpdate: area {AreaIndex}, to neutralize {ToNeutralize}", cappedEvent.AreaIdx, cappedEvent.ToNeutralize);
            }
            else if (args.GameEvent is VAreaPaintAreaContestedStateUpdateEvent)
            {
                VAreaPaintAreaContestedStateUpdateEvent contestedEvent = args.GameEvent as VAreaPaintAreaContestedStateUpdateEvent;
                LogContext.Information("VAreaPaintAreaContestedStateUpdate: area {AreaIndex}, favoured team {FavouredTeam}, to capture {ToCapture}", contestedEvent.AreaIdx, contestedEvent.FavouredTeam, contestedEvent.ToCapture);
            }
            else if (args.GameEvent is VAreaPaintAreaControlChangeEvent)
            {
                VAreaPaintAreaControlChangeEvent changeEvent = args.GameEvent as VAreaPaintAreaControlChangeEvent;
                LogContext.Information("VAreaPaintAreaControlChange: area {AreaIndex}, controlled team {Team}", changeEvent.AreaIdx, changeEvent.Team);
            }
            else if (args.GameEvent is PlayerGaugeUpdateEvent)
            {
                return;
            }
            else
            {
                string json = JsonSerializer.Serialize(args.GameEvent, args.GameEvent.GetType(), new JsonSerializerOptions()
                {
                    WriteIndented = true
                });

                string linePrefix = args.GameEvent.Name + ": {Json}";
                LogContext.Information(linePrefix, json);
            }
        }

    }
}
