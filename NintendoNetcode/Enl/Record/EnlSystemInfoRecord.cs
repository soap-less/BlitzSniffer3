using Syroot.BinaryData;
using System.Collections.Generic;

namespace NintendoNetcode.Enl.Record
{
    public class EnlSystemInfoRecord : EnlRecord
    {
        public ulong ConnectedBitmap
        {
            get;
            private set;
        }

        public ulong DisconnectedBitmap
        {
            get;
            private set;
        }

        public ulong Unknown1
        {
            get;
            private set;
        }

        public ulong ReceivedBitmap
        {
            get;
            private set;
        }

        public ulong SessionTime
        {
            get;
            private set;
        }

        public ulong PrincipalId
        {
            get;
            private set;
        }

        public byte Unknown2
        {
            get;
            private set;
        }

        public List<EnlUniqueId> Unknown3
        {
            get;
            private set;
        }

        public List<EnlUniqueId> Unknown4
        {
            get;
            private set;
        }

        public byte Unknown5
        {
            get;
            private set;
        }

        public List<uint> PlayerIds
        {
            get;
            private set;
        }

        internal EnlSystemInfoRecord(BinaryDataReader reader, int p, int q)
        {
            ConnectedBitmap = reader.ReadUInt64();
            DisconnectedBitmap = reader.ReadUInt64();
            Unknown1 = reader.ReadUInt64();
            ReceivedBitmap = reader.ReadUInt64();
            SessionTime = reader.ReadUInt64();
            PrincipalId = reader.ReadUInt64();
            Unknown2 = reader.ReadByte();

            Unknown3 = new List<EnlUniqueId>();
            for (int i = 0; i < ((p * 2) - 1); i++)
            {
                Unknown3.Add(new EnlUniqueId(reader));
            }

            Unknown4 = new List<EnlUniqueId>();
            for (int i = 0; i < q; i++)
            {
                Unknown4.Add(new EnlUniqueId(reader));
            }

            Unknown5 = reader.ReadByte();

            PlayerIds = new List<uint>();
            for (int i = 0; i < p; i++)
            {
                PlayerIds.Add(reader.ReadByte());
            }
        }

    }
}
