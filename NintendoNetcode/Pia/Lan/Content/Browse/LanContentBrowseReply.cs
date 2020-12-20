using NintendoNetcode.Pia.Lan.Content.Browse.Crypto;
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

        public LanCryptoContentResponse CryptoResponse
        {
            get;
            set;
        }

        public LanContentBrowseReply(BinaryDataReader reader) : base(reader)
        {
            uint sessionInfoSize = reader.ReadUInt32();
            SessionInfo = new LanSessionInfo(reader);
            CryptoResponse = new LanCryptoContentResponse(reader);
        }

    }
}
