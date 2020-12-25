using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    public abstract class CloneContentCloneSequentialMessage : CloneContentMessage
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

        protected CloneContentCloneSequentialMessage(BinaryDataReader reader) : base(reader)
        {
            SmhUnknownOne = reader.ReadUInt32();
            SmhUnknownTwo = reader.ReadUInt16();
        }

    }
}
