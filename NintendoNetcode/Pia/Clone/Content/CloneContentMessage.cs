using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    abstract class CloneContentMessage : CloneContent
    {
        public CloneType CloneType
        {
            get;
            set;
        }

        public byte StationIdx
        {
            get;
            set;
        }

        public ushort CmhUnknownThree
        {
            get;
            set;
        }

        public uint CloneId
        {
            get;
            set;
        }

        public CloneContentMessage(BinaryDataReader reader) : base(reader)
        {
            CloneType = (CloneType)reader.ReadByte();
            StationIdx = reader.ReadByte();
            CmhUnknownThree = reader.ReadUInt16();
            CloneId = reader.ReadUInt32();
        }

    }
}
