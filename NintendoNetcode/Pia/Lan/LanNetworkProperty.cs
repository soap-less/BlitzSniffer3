using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Text;

namespace NintendoNetcode.Pia.Lan
{
    public class LanNetworkProperty
    {
        public uint GameMode;
        public uint SessionId;
        public uint[] Attributes;
        public ushort ParticipantCount;
        public ushort MinParticipants;
        public ushort MaxParticipants;
        public byte SystemCommunicationVersion;
        public byte AppCommunicationVersion;
        public ushort SessionType;
        public byte[] ApplicationData;
        public bool IsOpened;
        public StationLocation Location;
        public List<LanStationInfo> Stations;
        public byte[] SessionParam;

        public LanNetworkProperty(BinaryDataReader reader)
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
