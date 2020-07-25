using Syroot.BinaryData;

namespace NintendoNetcode.Enl.Record
{
    public class EnlSystemRequestInfoRecord : EnlRecord
    {
        public uint GameRecords
        {
            get;
            private set;
        }

        public uint SystemRecords
        {
            get;
            private set;
        }

        internal EnlSystemRequestInfoRecord(BinaryDataReader reader)
        {
            GameRecords = reader.ReadUInt32();
            SystemRecords = reader.ReadUInt32();
        }

    }
}
