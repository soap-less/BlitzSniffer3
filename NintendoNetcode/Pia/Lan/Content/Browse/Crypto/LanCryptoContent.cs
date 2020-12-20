using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Lan.Content.Browse.Crypto
{
    public abstract class LanCryptoContent
    {
        public byte Version
        {
            get;
            set;
        }

        public bool CryptoEnabled
        {
            get;
            set;
        }

        public long Counter
        {
            get;
            set;
        }

        public byte[] ChallengeKey
        {
            get;
            set;
        }

        public byte[] Tag
        {
            get;
            set;
        }

        public byte[] Data
        {
            get;
            set;
        }

        protected LanCryptoContent()
        {

        }

        protected LanCryptoContent(BinaryDataReader reader)
        {
            Version = reader.ReadByte();
            CryptoEnabled = reader.ReadByte() == 0x1;
            Counter = reader.ReadInt64();
            ChallengeKey = reader.ReadBytes(16);
            Tag = reader.ReadBytes(16);
        }

        public virtual void Serialize(BinaryDataWriter writer)
        {
            writer.Write(Version);
            writer.Write((byte)(CryptoEnabled ? 0x1 : 0x0));
            writer.Write(Counter);
            writer.Write(ChallengeKey);
            writer.Write(Tag);
            writer.Write(Data);
        }

    }
}
