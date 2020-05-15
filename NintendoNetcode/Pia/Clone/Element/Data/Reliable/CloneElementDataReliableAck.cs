using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Element.Data.Reliable
{
    class CloneElementDataReliableAck : CloneElementDataReliable
    {
        public byte SetterStationIdx
        {
            get;
            set;
        }

        public uint Clock
        {
            get;
            set;
        }

        public CloneElementDataReliableAck(BinaryDataReader reader) : base(reader)
        {
            SetterStationIdx = reader.ReadByte();
            Clock = reader.ReadUInt32();
        }

    }
}
