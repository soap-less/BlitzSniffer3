using Syroot.BinaryData;
using System.IO;

namespace NintendoNetcode.Pia.Clone.Element.Data.Reliable
{
    abstract class CloneElementDataReliable : CloneElementData
    {
        private byte Type
        {
            get;
            set;
        }

        public CloneElementDataReliable(BinaryDataReader reader) : base(reader)
        {
            Type = reader.ReadByte();
        }

    }
}
