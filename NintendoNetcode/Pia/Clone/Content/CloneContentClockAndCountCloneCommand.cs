using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    public class CloneContentClockAndCountCloneCommand : CloneContentClockCloneCommand
    {
        public byte CountUnknownOne
        {
            get;
            set;
        }

        public byte CountUnknownTwo
        {
            get;
            set;
        }

        public ushort CountUnknownThree
        {
            get;
            set;
        }

        public CloneContentClockAndCountCloneCommand(BinaryDataReader reader) : base(reader)
        {
            CountUnknownOne = reader.ReadByte();
            CountUnknownTwo = reader.ReadByte();
            CountUnknownThree = reader.ReadUInt16();
        }

    }
}
