using Syroot.BinaryData;
using System.IO;

namespace NintendoNetcode.Pia.Clone.Element.Data.Event
{
    class CloneElementDataEventInitialAck : CloneElementDataEvent
    {
        public ushort Index
        {
            get;
            set;
        }

        public CloneElementDataEventInitialAck(BinaryDataReader reader) : base(reader)
        {
            reader.Seek(1, SeekOrigin.Begin); // padding
        }

    }
}
