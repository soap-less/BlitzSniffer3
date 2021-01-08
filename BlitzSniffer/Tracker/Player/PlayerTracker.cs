using Blitz.Cmn.Def;
using BlitzSniffer.Clone;
using BlitzSniffer.Event;
using BlitzSniffer.Event.Player;
using BlitzSniffer.Event.Player.VGoal;
using BlitzSniffer.Event.Player.VLift;
using BlitzSniffer.Resources;
using BlitzSniffer.Tracker.Station;
using BlitzSniffer.Tracker.Versus.VLift;
using BlitzSniffer.Util;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace BlitzSniffer.Tracker.Player
{
    class PlayerTracker : IDisposable
    {
        private readonly Dictionary<uint, Player> Players;
        private readonly PlayerOffenseTracker OffenseTracker;

        private uint TeamBits;

        public PlayerTracker()
        {
            Players = new Dictionary<uint, Player>();
            OffenseTracker = new PlayerOffenseTracker();

            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged += UpdatePlayerDetails;
            holder.CloneChanged += HandlePlayerEvent;
            holder.CloneChanged += HandlePlayerNetState;

            for (uint i = 0; i < 10; i++)
            {
                holder.RegisterClone(i + 111);

                Players[i] = new Player($"Player {i}");
            }
        }

        public void Dispose()
        {
            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged -= HandlePlayerEvent;
            holder.CloneChanged -= HandlePlayerNetState;
        }

        public Player GetPlayer(uint idx)
        {
            return Players[idx];
        }

        public Player GetPlayerWithGachihoko()
        {
            IEnumerable<Player> gachihokoPlayers = Players.Values.Where(p => p.HasGachihoko);
            Trace.Assert(gachihokoPlayers.Count() <= 1, "More than one player with Gachihoko");

            return gachihokoPlayers.FirstOrDefault();
        }

        public int GetAcivePlayers()
        {
            return Players.Values.Where(p => p.IsActive).Count();
        }

        public int GetPlayersOnVLift()
        {
            return Players.Values.Where(p => p.IsOnVLift).Count();
        }

        public void SetTeamBits(uint teamBits)
        {
            TeamBits = teamBits;
        }

        private void ApplyTeamBits()
        {
            uint neutralBits = TeamBits >> 16;
            uint actualTeamBits = TeamBits & 0xFFFF;

            for (uint i = 0; i != 10; i++)
            {
                uint mask = (uint)(1 << (int)i);

                Player player = Players[i];
                if ((neutralBits & mask) != 0 || !player.IsActive)
                {
                    player.Team = Team.Neutral;
                }
                else
                {
                    player.Team = (actualTeamBits & mask) != 0 ? Team.Bravo : Team.Alpha;
                }
            }
        }

        private void UpdatePlayerDetails(object sender, CloneChangedEventArgs args)
        {
            uint playerId = args.CloneId - 111;
            if (playerId >= 10)
            {
                return;
            }

            Player player = Players[playerId];

            if (!player.IsActive)
            {
                StationTracker tracker = GameSession.Instance.StationTracker;
                
                Station.Station station = GameSession.Instance.StationTracker.GetStationForSsid(args.SourceStationId);
                if (!station.IsSetup)
                {
                    return;
                }

                player.Name = station.Name;
                player.IsAlive = true;
                player.IsActive = true;

                if (Players.Values.Where(p => p.IsActive).Count() == tracker.ActivePlayerCount)
                {
                    // Apply all team bits now that all players have been marked as active
                    ApplyTeamBits();

                    GameSession.Instance.SignalSetupReady();
                }
            }
        }

        private void HandlePlayerEvent(object sender, CloneChangedEventArgs args)
        {
            uint playerId = args.CloneId - 111;
            if (playerId >= 10)
            {
                return;
            }

            if (args.ElementId != 1)
            {
                return;
            }

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                reader.Seek(15, SeekOrigin.Begin);
                uint unk7 = reader.ReadUInt32();
                uint unk8 = reader.ReadUInt32();

                BitReader bitReader = new BitReader(reader);
                uint eventId = bitReader.ReadVariableBits(7);
                uint unk10 = bitReader.ReadVariableBits(4) - 1;

                Player player = Players[playerId];

                // from Game::PlayerCloneHandle::unpackStateEvent()
                switch (eventId)
                {
                    case 1: // Killed
                    case 2: // OOB
                    case 3: // Water Hazard
                        if (!player.IsAlive)
                        {
                            return;
                        }

                        player.IsAlive = false;
                        player.Deaths++;

                        if (player.HasGachihoko)
                        {
                            // Not anymore.
                            player.HasGachihoko = false;

                            EventTracker.Instance.AddEvent(new PlayerLostGachihokoEvent()
                            {
                                PlayerIdx = playerId
                            });
                        }

                        int attackerIdx = -1;
                        string cause = "Unknown";
                        
                        if (eventId == 1)
                        {
                            uint type = (unk8 & 0xFF0000) >> 16;
                            uint id = unk8 & 0xFFFF;

                            cause = GetDeathCauseForAttackedPlayer(type, id);

                            // If this player was the Gachihoko holder and they died because of the timeout explosion,
                            // don't set their their attacker. Otherwise, the death event will say that they were killed
                            // by themselves, which isn't standard (for suicides, the attacker variable is never set).
                            if (type != 3 || id != 4)
                            {
                                attackerIdx = (int)unk10;
                            }
                        }
                        else if (eventId == 2)
                        {
                            cause = "Stg_OutOfBounds";
                        }
                        else if (eventId == 3)
                        {
                            cause = "Stg_Water";
                        }

                        PlayerDeathEvent deathEvent = OffenseTracker.GetDeathEventForVictim(playerId);
                        deathEvent.AttackerIdx = attackerIdx;
                        deathEvent.Cause = cause;
                        deathEvent.IsComplete = true;

                        break;
                    case 4: // Revival (after being splatted)
                    case 5: // Recover (after entering out of bounds or water)
                        if (player.IsAlive)
                        {
                            return;
                        }

                        player.IsAlive = true;

                        EventTracker.Instance.AddEvent(new PlayerRespawnEvent()
                        {
                            PlayerIdx = playerId
                        });

                        break;
                    case 6: // Assist
                        PlayerDeathEvent assistDeathEvent = OffenseTracker.GetDeathEventForVictim(unk10);
                        assistDeathEvent.AssisterIdx = (int)playerId;

                        break;
                    case 21:
                        // TODO: why does this sometimes fire when charge != 100?
                        // TODO: what happens when internal specials like BigLaser are activated?
                        if (player.SpecialGaugeCharge == 100)
                        {
                            EventTracker.Instance.AddEvent(new PlayerSpecialActivateEvent()
                            {
                                PlayerIdx = playerId
                            });
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        private void HandlePlayerNetState(object sender, CloneChangedEventArgs args)
        {
            uint playerId = args.CloneId - 111;
            if (playerId >= 10)
            {
                return;
            }

            if (args.ElementId != 0)
            {
                return;
            }

            Player player = Players[playerId];

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                BitReader bitReader = new BitReader(reader);

                // TODO: clean up

                // mPos
                bitReader.Seek(16 * 3);
                bitReader.Seek(1 * 3);

                // mMoveVel
                bitReader.Seek(11);
                bitReader.Seek(12);
                bitReader.Seek(12);

                // mJumpVel
                bitReader.Seek(12);

                // mJumpVel_Leak
                bitReader.Seek(12);

                // mPosY_Leap
                bitReader.Seek(17);

                // mReactVel
                bitReader.Seek(11);
                bitReader.Seek(12);
                bitReader.Seek(12);

                // mGndNrm_Raw
                bitReader.Seek(11);
                bitReader.Seek(12);

                // mAttDirZ
                bitReader.Seek(11);
                bitReader.Seek(12);

                // mShotDirXZ
                bitReader.Seek(14);

                // mUnk4
                bitReader.Seek(6);

                // mUnk5
                bitReader.Seek(2 + 10);

                // mUnk6
                bitReader.Seek(8);

                // special gauge
                uint charge = bitReader.ReadVariableBits(7);
                if (player.SpecialGaugeCharge != charge)
                {
                    player.SpecialGaugeCharge = charge;

                    EventTracker.Instance.AddEvent(new PlayerGaugeUpdateEvent()
                    {
                        PlayerIdx = playerId,
                        Charge = player.SpecialGaugeCharge
                    });
                }

                // mUnk8
                bitReader.Seek(10);

                // mUnk9
                bitReader.Seek(10);

                // mUnk10
                bitReader.Seek(17);

                // mUnk?? (not in 3.1.0)
                bitReader.Seek(17);

                // mUnk11
                bitReader.Seek(2 + 10);

                // mUnk12
                bitReader.Seek(9);

                // mUnk13
                bitReader.Seek(4);

                // flags
                uint flags = bitReader.ReadVariableBits(32);
                if (GameSession.Instance.GameStateTracker is VLiftVersusGameStateTracker)
                {
                    UpdateVLiftRidingStatus(playerId, (flags & 0x8000000) != 0);
                }
            }
        }

        // from Game::VersusBeatenPage::start()
        private string GetDeathCauseForAttackedPlayer(uint type, uint id)
        {
            string cause = $"Unknown ({type} - {id})";
            switch (type)
            {
                case 0:
                    cause = WeaponResource.Instance.GetMainWeapon((int)id);
                    break;
                case 1:
                    cause = WeaponResource.Instance.GetSubWeapon((int)id);
                    break;
                case 2:
                    cause = WeaponResource.Instance.GetSpecialWeapon((int)id);
                    break;
                case 3:
                    switch (id)
                    {
                        // Cases 4 to 7 were found in the array used for Cui::TextSetter::setLayoutMsg
                        case 4: // Gachihoko explosion (holder?)
                        case 6: // Gachihoko explosion (others?)
                            cause = "Wsp_Shachihoko_Explosion";
                            break;
                        case 5: // Gachihoko bullet
                            cause = "Wsp_Shachihoko";
                            break;
                        case 7: // Gachihoko barrier
                            cause = $"Wsp_Shachihoko_Barrier";
                            break;
                        case 8:
                            cause = "Wot_PCFan";
                            break;
                        case 11:
                            cause = "Wot_Geyser";
                            break;
                        case 12:
                            cause = "Wot_Takodozer";
                            break;
                        case 13:
                            cause = "Wot_RollingBarrel";
                            break;
                        case 14:
                            cause = "Wot_Blowouts";
                            break;
                        case 15:
                            cause = "Wsp_BigLaser";
                            break;
                        case 16:
                            cause = "Wot_IidaBomb";
                            break;
                    }
                    break;
                case 4: // Crushed
                    cause = WeaponResource.Instance.GetMainWeapon((int)id);
                    break;
                case 5:
                    cause = $"Wsp_Jetpack_Exhaust";
                    break;
                case 6: // Squished
                    cause = WeaponResource.Instance.GetMainWeapon((int)id);
                    break;
                case 7:
                    cause = $"Wsp_Shachihoko_Explosion";
                    break;
                case 9:
                case 13:
                case 14:
                    cause = WeaponResource.Instance.GetMainWeapon((int)id);
                    break;
                case 10:
                    cause = WeaponResource.Instance.GetSubWeapon((int)id);
                    break;
                case 11:
                case 12:
                    cause = WeaponResource.Instance.GetSpecialWeapon((int)id);
                    break;
            }

            return cause;
        }

        private void UpdateVLiftRidingStatus(uint id, bool riding)
        {
            Player player = Players[id];

            if (player.IsOnVLift != riding)
            {
                player.IsOnVLift = riding;

                if (riding)
                {
                    EventTracker.Instance.AddEvent(new PlayerRidingVLiftEvent()
                    {
                        PlayerIdx = id
                    });
                }
                else
                {
                    EventTracker.Instance.AddEvent(new PlayerLeftVLiftEvent()
                    {
                        PlayerIdx = id
                    });
                }

                VLiftVersusGameStateTracker tracker = GameSession.Instance.GameStateTracker as VLiftVersusGameStateTracker;
                tracker.UpdateOvertimeTimeoutState();
            }
        }

    }
}
