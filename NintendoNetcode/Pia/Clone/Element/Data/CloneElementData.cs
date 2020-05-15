using Syroot.BinaryData;
using System;

namespace NintendoNetcode.Pia.Clone.Element.Data
{
    abstract class CloneElementData
    {
        public int DataSize
        {
            get;
            set;
        }

        public ushort Id
        {
            get;
            set;
        }

        public CloneElementData(BinaryDataReader reader)
        {
            ushort typeAndSize = reader.ReadUInt16();
            DataSize = typeAndSize & 0x0FFF;
            Id = reader.ReadUInt16();
        }

    }
}
