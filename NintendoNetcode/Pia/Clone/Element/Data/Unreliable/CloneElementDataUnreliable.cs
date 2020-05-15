using Syroot.BinaryData;
using System.IO;

namespace NintendoNetcode.Pia.Clone.Element.Data.Unreliable
{
    class CloneElementDataUnreliable : CloneElementData
    {
        public uint Clock
        {
            get;
            set;
        }

        public byte[] Data
        {
            get;
            set;
        }

        public CloneElementDataUnreliable(BinaryDataReader reader) : base(reader)
        {
            Clock = reader.ReadUInt32();
            Data = reader.ReadBytes(DataSize - 8);
        }

    }
}
