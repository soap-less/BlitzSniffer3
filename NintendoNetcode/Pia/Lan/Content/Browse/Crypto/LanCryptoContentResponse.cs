using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Lan.Content.Browse.Crypto
{
    public class LanCryptoContentResponse : LanCryptoContent
    {
        public LanCryptoContentResponse(BinaryDataReader reader) : base(reader)
        {
            Data = reader.ReadBytes(16);
        }

    }
}
