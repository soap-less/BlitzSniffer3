using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Text;

namespace NintendoNetcode.Pia
{
    public class StationLocation
    {
        public InetAddress Address
        {
            get;
            set;
        }

        public ulong PrincipalId
        {
            get;
            set;
        }

        public uint CId
        {
            get;
            set;
        }

        public uint RvcId
        {
            get;
            set;
        }

        public byte UrlType
        {
            get;
            set;
        }

        public byte SId
        {
            get;
            set;
        }

        public byte Stream
        {
            get;
            set;
        }

        public byte NatM
        {
            get;
            set;
        }

        public byte NatF
        {
            get;
            set;
        }

        public byte Type
        {
            get;
            set;
        }

        public byte ProbeInit
        {
            get;
            set;
        }

        public InetAddress Relay
        {
            get;
            set;
        }

        public StationLocation(BinaryDataReader reader)
        {
            Address = new InetAddress(reader);
            PrincipalId = reader.ReadUInt64();
            CId = reader.ReadUInt32();
            RvcId = reader.ReadUInt32();
            UrlType = reader.ReadByte();
            SId = reader.ReadByte();
            Stream = reader.ReadByte();
            NatM = reader.ReadByte();
            NatF = reader.ReadByte();
            Type = reader.ReadByte();
            ProbeInit = reader.ReadByte();
            Relay = new InetAddress(reader);
        }

    }
}
