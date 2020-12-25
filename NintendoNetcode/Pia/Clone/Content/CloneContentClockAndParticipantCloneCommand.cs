using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    public class CloneContentClockAndParticipantCloneCommand : CloneContentClockCloneCommand
    {
        public uint ParticipantUnknownOne
        {
            get;
            set;
        }

        public CloneContentClockAndParticipantCloneCommand(BinaryDataReader reader) : base(reader)
        {
            ParticipantUnknownOne = reader.ReadUInt32();
        }

    }
}

