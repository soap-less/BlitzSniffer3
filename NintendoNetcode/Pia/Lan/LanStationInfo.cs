using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Text;

namespace NintendoNetcode.Pia.Lan
{
    public class LanStationInfo
    {
        public byte Role
        {
            get;
            set;
        }

        public byte EncodingType
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public ulong StationId
        {
            get;
            set;
        }

        public LanStationInfo(BinaryDataReader reader)
        {
            Role = reader.ReadByte();
            EncodingType = reader.ReadByte();
            Name = reader.ReadString(40, EncodingType == 0x1 ? Encoding.UTF8 : Encoding.Unicode);
            StationId = reader.ReadUInt64();
        }

    }
}
