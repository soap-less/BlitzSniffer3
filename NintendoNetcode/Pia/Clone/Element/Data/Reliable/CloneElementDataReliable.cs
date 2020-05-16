using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Element.Data.Reliable
{
    public abstract class CloneElementDataReliable : CloneElementData
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
