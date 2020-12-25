using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    public class CloneContentClockReply : CloneContentSequentialMessage
    {
        public uint Clock
        {
            get;
            set;
        }

        public ulong UnknownTwo
        {
            get;
            set;
        }

        public CloneContentClockReply(BinaryDataReader reader) : base(reader)
        {
            Clock = reader.ReadUInt32();
            UnknownTwo = reader.ReadUInt64();
        }

    }
}
