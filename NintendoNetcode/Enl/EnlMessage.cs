using NintendoNetcode.Enl.Record;
using Syroot.BinaryData;
using System.Collections.Generic;

namespace NintendoNetcode.Enl
{
    public class EnlMessage
    {
        public List<EnlRecord> Records
        {
            get;
            private set;
        }

        public EnlMessage(BinaryDataReader reader, int p, int q)
        {
            Records = new List<EnlRecord>();

            bool readAll = false;
            while (!readAll)
            {
                byte recordType = reader.ReadByte();
                ushort recordSize = reader.ReadUInt16();

                switch (recordType)
                {
                    case 253:
                        Records.Add(new EnlSystemRequestInfoRecord(reader));
                        break;
                    case 254:
                        Records.Add(new EnlSystemInfoRecord(reader, p, q));
                        break;
                    case 255: // end record
                        readAll = true;
                        break;
                    default:
                        Records.Add(new EnlGameRecord(reader, recordSize));
                        break;
                }
            }
        }

    }
}
