using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Text;

namespace NintendoNetcode.Pia.Lan
{
    public class LanSessionInfo
    {
        public uint GameMode
        {
            get;
            set;
        }

        public uint SessionId
        {
            get;
            set;
        }

        public uint[] Attributes
        {
            get;
            set;
        }

        public ushort ParticipantCount
        {
            get;
            set;
        }

        public ushort MinParticipants
        {
            get;
            set;
        }

        public ushort MaxParticipants
        {
            get;
            set;
        }

        public byte SystemCommunicationVersion
        {
            get;
            set;
        }

        public byte AppCommunicationVersion
        {
            get;
            set;
        }

        public ushort SessionType
        {
            get;
            set;
        }

        public byte[] ApplicationData
        {
            get;
            set;
        }

        public bool IsOpened
        {
            get;
            set;
        }

        public StationLocation Location
        {
            get;
            set;
        }

        public List<LanStationInfo> Stations
        {
            get;
            set;
        }

        public byte[] SessionParam
        {
            get;
            set;
        }

        public LanSessionInfo(BinaryDataReader reader)
        {
            GameMode = reader.ReadUInt32();
            SessionId = reader.ReadUInt32();
            Attributes = reader.ReadUInt32s(6);
            ParticipantCount = reader.ReadUInt16();
            MinParticipants = reader.ReadUInt16();
            MaxParticipants = reader.ReadUInt16();
            SystemCommunicationVersion = reader.ReadByte();
            AppCommunicationVersion = reader.ReadByte();
            SessionType = reader.ReadUInt16();
            ApplicationData = reader.ReadBytes(0x180);
            uint dataSize = reader.ReadUInt32(); // why is this field *after* the data?
            IsOpened = reader.ReadByte() == 0x1;
            Location = new StationLocation(reader);

            Stations = new List<LanStationInfo>();
            for (int i = 0; i < 16; i++)
            {
                Stations.Add(new LanStationInfo(reader));
            }

            SessionParam = reader.ReadBytes(0x20);
        }

    }
}
