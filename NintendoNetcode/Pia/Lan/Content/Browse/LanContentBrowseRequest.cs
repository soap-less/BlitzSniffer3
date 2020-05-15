using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Lan.Content.Browse
{
    public class LanContentBrowseRequest : LanContent
    {
        public byte[] SessionSearchCriteria
        {
            get;
            set;
        }

        public byte[] CryptoChallenge
        {
            get;
            set;
        }

        public LanContentBrowseRequest(BinaryDataReader reader) : base(reader)
        {

        }

        protected override void Deserialize(BinaryDataReader reader)
        {
            uint criteriaSize = reader.ReadUInt32();
            SessionSearchCriteria = reader.ReadBytes((int)criteriaSize);
            CryptoChallenge = reader.ReadBytes(0x12a);
        }

    }
}
