using Blitz.Cmn.Def;
using BlitzSniffer.Clone;
using BlitzSniffer.Event;
using BlitzSniffer.Event.Player;
using BlitzSniffer.Util;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BlitzSniffer.Tracker.Player
{
    class PlayerTracker : IDisposable
    {
        private readonly Dictionary<uint, Player> Players;
        private readonly PlayerOffenseTracker OffenseTracker;

        public PlayerTracker()
        {
            Players = new Dictionary<uint, Player>();
            OffenseTracker = new PlayerOffenseTracker();

            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged += HandlePlayerName;
            holder.CloneChanged += HandlePlayerEvent;

            for (uint i = 0; i < 10; i++)
            {
                holder.RegisterClone(i + 3);
                holder.RegisterClone(i + 111);

                Players[i] = new Player($"Player {i}");
            }
        }

        public void Dispose()
        {
            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged -= HandlePlayerName;
            holder.CloneChanged -= HandlePlayerEvent;
        }

        public Player GetPlayer(uint idx)
        {
            return Players[idx];
        }

        public void SetTeams(uint teamBits)
        {
            ushort neutralBits = (ushort)(teamBits >> 16);
            ushort actualTeamBits = (ushort)(teamBits & 0xFFFF);

            for (uint i = 0; i != 10; i++)
            {
                ushort mask = (ushort)(1 << (int)i);

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

        private void HandlePlayerName(object sender, CloneChangedEventArgs args)
        {
            uint playerId = args.CloneId - 3;
            if (playerId > 10)
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
                string name = reader.ReadString(BinaryStringFormat.ZeroTerminated, Encoding.Unicode);

                Players[playerId].Name = name;

                if (!Players[playerId].IsActive)
                {
                    Players[playerId].IsActive = true;
                    Players[playerId].IsAlive = true;

                    Console.WriteLine($"[PlayerTracker] registered {name}");
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

                        string cause = "Unknown";
                        if (eventId == 1)
                        {
                            cause = GetDeathCauseForAttackedPlayer((unk8 & 0xFF0000) >> 16, unk8 & 0xFFFF);
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
                        deathEvent.AttackerIdx = eventId == 1 ? (int)unk10 : -1;
                        deathEvent.Cause = cause;
                        deathEvent.IsComplete = true;

                        break;
                    case 4: // Respawn
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
                    default:
                        break;
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
                case 1:
                case 2:
                    cause = $"type {type} id {id}";
                    break;
                case 3:
                    switch (id)
                    {
                        case 4:
                        case 5:
                        case 6:
                        case 7:
                            cause = "Wsp_Shachihoko";
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
                case 4:
                    cause = $"type crushed (main) id {id}";
                    break;
                case 5:
                    cause = $"Wsp_Jetpack_Exhaust";
                    break;
                case 6:
                    cause = $"type squished (main) id {id}";
                    break;
                case 7:
                    cause = $"Wsp_Shachihoko_Explosion";
                    break;
                case 9:
                case 13:
                case 14:
                    cause = $"type main id {id}";
                    break;
                case 10:
                    cause = $"type sub id {id}";
                    break;
                case 11:
                case 12:
                    cause = $"type special id {id}";
                    break;
            }

            return cause;
        }

    }
}
