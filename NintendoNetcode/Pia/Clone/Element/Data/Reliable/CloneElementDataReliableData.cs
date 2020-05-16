using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Element.Data.Reliable
{
    public class CloneElementDataReliableData : CloneElementDataReliable
    {
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

        public byte[] Data
        {
            get;
            set;
        }

        public CloneElementDataReliableData(BinaryDataReader reader) : base(reader)
        {
            SetterStationIdx = reader.ReadByte();
            DestBitmap = reader.ReadUInt32();
            Clock = reader.ReadUInt32();
            Data = reader.ReadBytes(DataSize - 14);
        }

    }
}
