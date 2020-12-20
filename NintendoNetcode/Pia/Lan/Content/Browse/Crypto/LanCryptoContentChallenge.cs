using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Lan.Content.Browse.Crypto
{
    public class LanCryptoContentChallenge : LanCryptoContent
    {
        public LanCryptoContentChallenge() : base()
        {
        }

        public LanCryptoContentChallenge(BinaryDataReader reader) : base(reader)
        {
            Data = reader.ReadBytes(256);
        }

    }
}
