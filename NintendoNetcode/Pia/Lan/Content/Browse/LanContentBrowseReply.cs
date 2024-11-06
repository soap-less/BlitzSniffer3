using NintendoNetcode.Pia.Lan.Content.Browse.Crypto;
using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Lan.Content.Browse
{
    public class LanContentBrowseReply : LanContent
    {
        public LanNetworkProperty NetworkProperty
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
            uint networkPropertySize = reader.ReadUInt32();
            NetworkProperty = new LanNetworkProperty(reader);
            CryptoResponse = new LanCryptoContentResponse(reader);
        }

    }
}
