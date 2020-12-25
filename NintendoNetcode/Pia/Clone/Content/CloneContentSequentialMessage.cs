using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    public abstract class CloneContentSequentialMessage : CloneContent
    {
        public uint SmhUnknownOne
        {
            get;
            set;
        }

        public ushort SmhUnknownTwo
        {
            get;
            set;
        }

        protected CloneContentSequentialMessage(BinaryDataReader reader) : base(reader)
        {
            SmhUnknownOne = reader.ReadUInt32();
            SmhUnknownTwo = reader.ReadUInt16();
        }

    }
}
