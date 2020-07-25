using Syroot.BinaryData;

namespace NintendoNetcode.Enl.Record
{
    public class EnlGameRecord : EnlRecord
    {
        public byte[] Data
        {
            get;
            private set;
        }

        internal EnlGameRecord(BinaryDataReader reader, ushort size)
        {
            Data = reader.ReadBytes((int)size);
        }

    }
}
