using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    // Seriously, Nintendo - WTF?
    public class CloneContentClockAndCountAndParticipantCloneCommand : CloneContentClockAndCountCloneCommand
    {
        public uint ParticipantUnknownOne
        {
            get;
            set;
        }

        public CloneContentClockAndCountAndParticipantCloneCommand(BinaryDataReader reader) : base(reader)
        {
            ParticipantUnknownOne = reader.ReadUInt32();
        }

    }
}

