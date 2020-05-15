using Syroot.BinaryData;

namespace NintendoNetcode.Pia
{
    public class GenericMessage : PiaMessage
    {
        public byte[] Data
        {
            get;
            set;
        }

        public GenericMessage(BinaryDataReader reader) : base(reader)
        {
            Data = reader.ReadBytes(PayloadSize);
        }

    }
}
