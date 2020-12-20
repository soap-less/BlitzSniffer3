using Syroot.BinaryData;
using System;

namespace NintendoNetcode.Pia.Lan.Content
{
    public abstract class LanContent
    {
        public LanContent(BinaryDataReader reader)
        {

        }

        public virtual void Serialize(BinaryDataWriter writer)
        {
            throw new NotSupportedException("Can't serialize this LanContent");
        }

    }
}
