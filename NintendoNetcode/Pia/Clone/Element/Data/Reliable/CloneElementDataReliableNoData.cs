using Syroot.BinaryData;
using System.IO;

namespace NintendoNetcode.Pia.Clone.Element.Data.Reliable
{
    class CloneElementDataReliableNoData : CloneElementDataReliable
    {
        public CloneElementDataReliableNoData(BinaryDataReader reader) : base(reader)
        {
            reader.Seek(1, SeekOrigin.Current); // padding
        }

    }
}
