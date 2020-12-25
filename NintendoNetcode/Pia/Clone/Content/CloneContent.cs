using Syroot.BinaryData;
using System;

namespace NintendoNetcode.Pia.Clone.Content
{
    public abstract class CloneContent
    {
        public byte MhUnknownOne
        {
            get;
            set;
        }

        public byte RawContentType
        {
            get;
            set;
        }

        public ushort MhUnknownTwo
        {
            get;
            set;
        }

        public CloneContent(BinaryDataReader reader)
        {
            MhUnknownOne = reader.ReadByte();
            RawContentType = reader.ReadByte();
            MhUnknownTwo = reader.ReadUInt16();
        }

        public static Type CloneContentClassTypeForType(CloneContentType type)
        {
            switch (type)
            {
                case CloneContentType.ClockReply:
                    return typeof(CloneContentClockReply);
                case CloneContentType.Data:
                    return typeof(CloneContentDataOnly);
                case CloneContentType.DataWithDestIndex:
                    return typeof(CloneContentDataWithDestIndex);
                case CloneContentType.DataWithDestBitmap:
                    return typeof(CloneContentDataWithDestBitmap);
                default:
                    return null;
            }
        }

    }
}
