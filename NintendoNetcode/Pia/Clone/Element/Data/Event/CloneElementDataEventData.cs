using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Element.Data.Event
{
    class CloneElementDataEventData : CloneElementDataEvent
    {
        public ushort Index
        {
            get;
            set;
        }

        public ushort EraseIndex
        {
            get;
            set;
        }

        public byte SetterStationIdx
        {
            get;
            set;
        }

        public uint DestBitmap
        {
            get;
            set;
        }

        public uint Clock
        {
            get;
            set;
        }

        public ulong RegisterCountAll
        {
            get;
            set;
        }

        public byte[] Data
        {
            get;
            set;
        }

        public CloneElementDataEventData(BinaryDataReader reader) : base(reader)
        {
            Index = reader.ReadUInt16();
            EraseIndex = reader.ReadUInt16();
            SetterStationIdx = reader.ReadByte();
            DestBitmap = reader.ReadUInt32();
            Clock = reader.ReadUInt32();
            RegisterCountAll = reader.ReadUInt64();
            Data = reader.ReadBytes(DataSize - 26);
        }

    }
}
