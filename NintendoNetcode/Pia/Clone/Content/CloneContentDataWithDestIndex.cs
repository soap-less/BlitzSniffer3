using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    class CloneContentDataWithDestIndex : CloneContentData
    {
        public byte DwdiUnknown
        {
            get;
            set;
        }

        public CloneContentDataWithDestIndex(BinaryDataReader reader) : base(reader)
        {
            DwdiUnknown = reader.ReadByte();

            DeserializeData(reader);
        }

    }
}
