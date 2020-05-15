using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    class CloneContentDataWithDestBitmap : CloneContentData
    {
        public ushort DwdbUnknown
        {
            get;
            set;
        }

        public CloneContentDataWithDestBitmap(BinaryDataReader reader) : base(reader)
        {
            DwdbUnknown = reader.ReadUInt16();

            DeserializeData(reader);
        }

    }
}
