using Syroot.BinaryData;

namespace NintendoNetcode.Enl
{
    public class EnlUniqueId
    {
        public ulong StationId
        {
            get;
            private set;
        }

        public ushort Unknown
        {
            get;
            private set;
        }

        internal EnlUniqueId(BinaryDataReader reader)
        {
            StationId = reader.ReadUInt64();
            Unknown = reader.ReadUInt16();
            reader.Seek(6); // padding
        }

    }
}
