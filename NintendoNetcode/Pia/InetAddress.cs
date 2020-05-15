using Syroot.BinaryData;

namespace NintendoNetcode.Pia
{
    public class InetAddress
    {
        public uint Address
        {
            get;
            set;
        }

        public ushort Port
        {
            get;
            set;
        }

        public InetAddress(BinaryDataReader reader)
        {
            Address = reader.ReadUInt32();
            Port = reader.ReadUInt16();
        }
    }
}
