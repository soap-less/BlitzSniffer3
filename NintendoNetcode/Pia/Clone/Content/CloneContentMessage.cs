using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    abstract class CloneContentMessage : CloneContent
    {
        public byte CmhUnknownOne
        {
            get;
            set;
        }

        public byte CmhUnknownTwo
        {
            get;
            set;
        }

        public ushort CmhUnknownThree
        {
            get;
            set;
        }

        public uint CmhUnknownFour
        {
            get;
            set;
        }

        public CloneContentMessage(BinaryDataReader reader) : base(reader)
        {
            CmhUnknownOne = reader.ReadByte();
            CmhUnknownTwo = reader.ReadByte();
            CmhUnknownThree = reader.ReadUInt16();
            CmhUnknownFour = reader.ReadUInt32();
        }

    }
}
