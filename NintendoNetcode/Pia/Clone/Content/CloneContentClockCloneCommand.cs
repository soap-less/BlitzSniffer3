using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    public class CloneContentClockCloneCommand : CloneContentCloneSequentialMessage
    {
        public uint Clock
        {
            get;
            set;
        }

        public CloneContentClockCloneCommand(BinaryDataReader reader) : base(reader)
        {
            Clock = reader.ReadUInt32();
        }

    }
}
