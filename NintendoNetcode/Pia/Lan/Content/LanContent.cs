using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Lan.Content
{
    public abstract class LanContent
    {
        public LanContent(BinaryDataReader reader)
        {
            Deserialize(reader);
        }

        protected abstract void Deserialize(BinaryDataReader reader);

    }
}
