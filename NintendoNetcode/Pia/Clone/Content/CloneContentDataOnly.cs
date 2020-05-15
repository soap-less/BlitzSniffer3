using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Content
{
    class CloneContentDataOnly : CloneContentData
    {
        public CloneContentDataOnly(BinaryDataReader reader) : base(reader)
        {
            DeserializeData(reader);
        }

    }
}
