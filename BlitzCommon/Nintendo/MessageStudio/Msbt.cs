using BlitzCommon.Util;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace BlitzCommon.Nintendo.MessageStudio
{
    public sealed class Msbt
    {
        private Dictionary<string, string> Values = new Dictionary<string, string>();
        private Dictionary<string, int> LabelValueIdx = new Dictionary<string, int>();
        private List<string> Strings = new List<string>();

        public IEnumerable<string> Keys
        {
            get
            {
                return Values.Keys;
            }
        }

        public Msbt(byte[] rawSarc)
        {
            using (MemoryStream memoryStream = new MemoryStream(rawSarc))
            {
                Read(memoryStream);
            }
        }

        public Msbt(Stream stream)
        {
            Read(stream);
        }

        public bool ContainsKey(string label)
        {
            return Values.ContainsKey(label);
        }

        public string Get(string label)
        {
            return Values[label];
        }

        // Based on MSBT file format documentation on ZeldaMods
        private void Read(Stream stream)
        {
            using BinaryDataReader reader = new BinaryDataReader(stream);

            // Set endianness to big by default
            reader.ByteOrder = ByteOrder.BigEndian;

            // Verify the magic numbers
            if (reader.ReadString(8) != "MsgStdBn")
            {
                throw new Exception("Not a MSBT file");
            }

            // Read BOM
            if (reader.ReadByte() == 0xFF && reader.ReadByte() == 0xFE)
            {
                reader.ByteOrder = ByteOrder.LittleEndian;
            }

            reader.Seek(2); // padding?

            ushort version = reader.ReadUInt16();
            ushort sectionCount = reader.ReadUInt16();

            reader.Seek(2); // padding?

            uint fileSize = reader.ReadUInt32();

            reader.Seek(10); // padding?

            for (int i = 0; i < sectionCount; i++)
            {
                Trace.Assert(reader.Position % 0x10 == 0);

                string sectionMagic = reader.ReadString(4);
                uint sectionSize = reader.ReadUInt32();

                reader.Seek(8); // padding

                long sectionStart = reader.Position;

                switch (sectionMagic)
                {
                    case "LBL1":
                        ReadLabelsSection(reader);
                        break;
                    case "TXT2":
                        ReadTextSection(reader);
                        break;
                    default:
                        // ATR1 not implemented
                        break;
                }

                // Seek to next table
                reader.Seek(sectionStart + sectionSize, SeekOrigin.Begin);

                long roundedOffset = MathUtil.RoundUpToMultiple((int)reader.Position, 0x10);
                reader.Seek(roundedOffset, SeekOrigin.Begin);
            }

            foreach (KeyValuePair<string, int> labelPair in LabelValueIdx)
            {
                Values[labelPair.Key] = Strings[labelPair.Value];
            }
        }

        private void ReadLabelsSection(BinaryDataReader reader)
        {
            long startOffset = reader.Position;

            uint offsetCount = reader.ReadUInt32();

            for (uint i = 0; i < offsetCount; i++)
            {
                uint stringCount = reader.ReadUInt32();
                uint stringOffset = reader.ReadUInt32();

                using (reader.TemporarySeek(startOffset + stringOffset, SeekOrigin.Begin))
                {
                    for (uint j = 0; j < stringCount; j++)
                    {
                        int stringLength = reader.ReadByte();
                        string label = reader.ReadString(stringLength);
                        int textTableIndex = (int)reader.ReadUInt32();

                        LabelValueIdx[label] = textTableIndex;
                    }
                }
            }
        }

        private void ReadTextSection(BinaryDataReader reader)
        {
            long startOffset = reader.Position;

            uint offsetCount = reader.ReadUInt32();

            for (uint i = 0; i < offsetCount; i++)
            {
                uint stringOffset = reader.ReadUInt32();

                using (reader.TemporarySeek(startOffset + stringOffset, SeekOrigin.Begin))
                {
                    string label = reader.ReadString(BinaryStringFormat.ZeroTerminated, Encoding.Unicode);

                    Strings.Add(label);
                }
            }
        }

    }
}
