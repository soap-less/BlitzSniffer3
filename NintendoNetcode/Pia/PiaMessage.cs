using NintendoNetcode.Pia.Clone;
using Syroot.BinaryData;
using System;
using System.IO;

namespace NintendoNetcode.Pia
{
    public abstract class PiaMessage
    {
        public byte Flags
        {
            get;
            set;
        }

        public ushort PayloadSize
        {
            get;
            set;
        }

        public ulong Destination
        {
            get;
            set;
        }

        public ulong SourceStationId
        {
            get;
            set;
        }

        public PiaProtocol ProtocolId
        {
            get;
            set;
        }

        public byte ProtocolPort
        {
            get;
            set;
        }

        public PiaMessage(BinaryDataReader reader)
        {
            Flags = reader.ReadByte();
            PayloadSize = reader.ReadUInt16();
            Destination = reader.ReadUInt64();
            SourceStationId = reader.ReadUInt64();
            ProtocolId = (PiaProtocol)reader.ReadByte();
            ProtocolPort = reader.ReadByte();
            reader.Seek(3, SeekOrigin.Current); // padding
        }

        public static Type PiaMessageForProtocol(PiaProtocol protocol)
        {
            switch (protocol)
            {
                case PiaProtocol.Clone:
                    return typeof(CloneMessage);
                default:
                    return typeof(GenericMessage);
            }
        }

    }
}
