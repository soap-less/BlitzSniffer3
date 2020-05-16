using NintendoNetcode.Pia.Clone.Content;
using Syroot.BinaryData;
using System;
using System.IO;

namespace NintendoNetcode.Pia.Clone
{
    public class CloneMessage : PiaMessage
    {
        public CloneContent Content
        {
            get;
            set;
        }

        public CloneMessage(BinaryDataReader reader) : base(reader)
        {
            CloneContentType contentType;
            using (reader.TemporarySeek(1))
            {
                contentType = (CloneContentType)(reader.ReadByte() & 0xF0);
            }

            Type type = CloneContent.CloneContentClassTypeForType(contentType);
            if (type == null)
            {
                reader.Seek(PayloadSize, SeekOrigin.Current);
                return;
            }

            byte[] data = reader.ReadBytes(PayloadSize);

            using (MemoryStream memoryStream = new MemoryStream(data))
            using (BinaryDataReader innerReader = new BinaryDataReader(memoryStream))
            {
                innerReader.ByteOrder = ByteOrder.BigEndian;

                Content = (CloneContent)Activator.CreateInstance(type, innerReader);
            }
        }

    }
}
