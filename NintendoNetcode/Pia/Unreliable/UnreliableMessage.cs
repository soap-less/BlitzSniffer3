using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Unreliable
{
    public class UnreliableMessage : PiaMessage
    {
        public byte[] Data
        {
            get;
            private set;
        }

        public UnreliableMessage(BinaryDataReader reader) : base(reader)
        {
            Data = reader.ReadBytes(PayloadSize);
        }

    }
}
