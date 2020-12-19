using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Lan.Content.Browse
{
    public class LanContentBrowseReply : LanContent
    {
        public LanSessionInfo SessionInfo
        {
            get;
            set;
        }

        public byte[] CryptoChallengeReply
        {
            get;
            set;
        }

        public LanContentBrowseReply(BinaryDataReader reader) : base(reader)
        {
            uint sessionInfoSize = reader.ReadUInt32();
            SessionInfo = new LanSessionInfo(reader);
            CryptoChallengeReply = reader.ReadBytes(0x3a);
        }

    }
}
