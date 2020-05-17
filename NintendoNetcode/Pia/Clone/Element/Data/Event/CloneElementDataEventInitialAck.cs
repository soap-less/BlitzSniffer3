using Syroot.BinaryData;
using System.IO;

namespace NintendoNetcode.Pia.Clone.Element.Data.Event
{
    class CloneElementDataEventInitialAck : CloneElementDataEvent
    {
        public CloneElementDataEventInitialAck(BinaryDataReader reader) : base(reader)
        {

        }

    }
}
