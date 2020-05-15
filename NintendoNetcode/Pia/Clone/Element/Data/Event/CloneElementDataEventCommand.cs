using Syroot.BinaryData;

namespace NintendoNetcode.Pia.Clone.Element.Data.Event
{
    class CloneElementDataEventCommand : CloneElementDataEvent
    {
        public ushort Index
        {
            get;
            set;
        }

        public CloneElementDataEventCommand(BinaryDataReader reader) : base(reader)
        {
            Index = reader.ReadUInt16();
        }

    }
}
