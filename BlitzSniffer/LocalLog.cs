using BlitzSniffer.Event;
using BlitzSniffer.Event.Player;
using BlitzSniffer.Event.Player.VGoal;
using BlitzSniffer.Event.Player.VLift;
using BlitzSniffer.Event.Versus;
using BlitzSniffer.Event.Versus.VArea;
using BlitzSniffer.Event.Versus.VGoal;
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
            switch (args.GameEvent)
            {
                case PlayerDeathEvent deathEvent:
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

                    break;
                case PlayerRespawnEvent respawnEvent:
                    LogContext.Information("PlayerRespawn: {Name} respawned", GameSession.Instance.PlayerTracker.GetPlayer(respawnEvent.PlayerIdx).Name);

                    break;
                case PlayerSpecialActivateEvent specialEvent:
                    LogContext.Information("PlayerSpecialActivate: {Name} activated their special weapon.", GameSession.Instance.PlayerTracker.GetPlayer(specialEvent.PlayerIdx).Name);

                    break;
                case PlayerGetGachihokoEvent getGachihokoEvent:
                    LogContext.Information("PlayerGetGachihokoEvent: {Name} got the Rainmaker", GameSession.Instance.PlayerTracker.GetPlayer(getGachihokoEvent.PlayerIdx).Name);

                    break;
                case PlayerLostGachihokoEvent lostGachihokoEvent:
                    LogContext.Information("PlayerLostGachihokoEvent: {Name} lost the Rainmaker", GameSession.Instance.PlayerTracker.GetPlayer(lostGachihokoEvent.PlayerIdx).Name);

                    break;
                case PlayerRidingVLiftEvent ridingEvent:
                    LogContext.Information("PlayerRidingVLiftEvent: {Name} is riding the Tower", GameSession.Instance.PlayerTracker.GetPlayer(ridingEvent.PlayerIdx).Name);

                    break;
                case PlayerLeftVLiftEvent leftEvent:
                    LogContext.Information("PlayerLeftVLiftEvent: {Name} is no longer riding the Tower", GameSession.Instance.PlayerTracker.GetPlayer(leftEvent.PlayerIdx).Name);

                    break;
                case GachiScoreUpdateEvent scoreUpdateEvent:
                    LogContext.Information("GachiScoreUpdate: alpha {AlphaScore} + {AlphaPenalty}, bravo {BravoScore} + {BravoPenalty}", scoreUpdateEvent.AlphaScore, scoreUpdateEvent.AlphaPenalty, scoreUpdateEvent.BravoScore, scoreUpdateEvent.BravoPenalty);
                    
                    break;
                case GachiOvertimeStartEvent overtimeStartEvent:
                    LogContext.Information("GachiOvertimeStart: overtime is starting now");
                    
                    break;
                case GachiOvertimeTimeoutUpdateEvent overtimeTimeoutEvent:
                    LogContext.Information("GachiOvertimeTimeoutUpdate: overtime will end in {Ticks} ticks", overtimeTimeoutEvent.Length);
                    
                    break;
                case GachiFinishEvent finishEvent:
                    LogContext.Information("GachiFinishEvent: game finish, {AlphaScore} - {BravoScore}", finishEvent.AlphaScore, finishEvent.BravoScore);

                    break;
                case VGoalBarrierBreakEvent barrierBreakEvent:
                    LogContext.Information("VGoalBarrierBreakEvent: Rainmaker barrier was broken by {Name}", GameSession.Instance.PlayerTracker.GetPlayer(barrierBreakEvent.BreakerPlayerIdx).Name);

                    break;
                /*case VLiftPositionUpdateEvent positionEvent:
                    LogContext.Information("VLiftPositionUpdate: alpha {AlphaPosition}%, bravo {BravoPosition}%", positionEvent.AlphaPosition, positionEvent.BravoPosition);
                
                    break;*/
                case VLiftCheckpointUpdateEvent checkpointUpdateEvent:
                    LogContext.Information("VLiftCheckpointUpdate: team {Team}, checkpoint {Index}, HP {Hp}, best HP {BestHp}", checkpointUpdateEvent.Team, checkpointUpdateEvent.Idx, checkpointUpdateEvent.Hp, checkpointUpdateEvent.BestHp);

                    break;
                case VAreaPaintAreaCappedStateUpdateEvent cappedEvent:
                    LogContext.Information("VAreaPaintAreaCappedStateUpdate: area {AreaIndex}, capped team has {Paint}% of the area", cappedEvent.AreaIdx, cappedEvent.PaintPercentage * 100f);

                    break;
                case VAreaPaintAreaContestedStateUpdateEvent contestedEvent:
                    LogContext.Information("VAreaPaintAreaContestedStateUpdate: area {AreaIndex}, paint {Paint}% in favour of {FavouredTeam}", contestedEvent.AreaIdx, contestedEvent.PaintPercentage * 100f, contestedEvent.FavouredTeam);
                    
                    break;
                case VAreaPaintAreaControlChangeEvent changeEvent:
                    LogContext.Information("VAreaPaintAreaControlChange: area {AreaIndex}, controlled team {Team}", changeEvent.AreaIdx, changeEvent.Team);

                    break;
                case PlayerGaugeUpdateEvent gaugeEvent:
                    break;
                default:
                    string json = JsonSerializer.Serialize(args.GameEvent, args.GameEvent.GetType(), new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    });

                    string linePrefix = args.GameEvent.Name + ": {Json}";
                    LogContext.Information(linePrefix, json);

                    break;
            }
        }

    }
}
