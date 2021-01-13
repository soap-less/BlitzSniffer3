using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Syroot.BinaryData;

namespace Nintendo.Archive
{
    public class Sarc : IEnumerable<string>, IDisposable
    {
        struct SfatNode
        {
            public uint Hash;
            public bool HasName;
            public int NameOfs;
            public uint DataOfs;
            public uint DataLength;
        }

        private uint DataStartOfs;
        private Dictionary<string, SfatNode> Files;
        private BinaryDataReader Reader;

        public byte[] this[string key]
        {
            get
            {
                return ReadFile(key);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Sarc(byte[] rawSarc)
        {
            using (MemoryStream memoryStream = new MemoryStream(rawSarc))
            {
                Read(memoryStream);
            }
        }

        public Sarc(Stream stream)
        {
            Read(stream);
        }

        private void Read(Stream stream)
        {
            // Create a dictionary to hold the files
            Files = new Dictionary<string, SfatNode>();

            Reader = new BinaryDataReader(stream);

            // Set endianness to big by default
            Reader.ByteOrder = ByteOrder.BigEndian;

            // Verify the magic numbers
            if (Reader.ReadString(4) != "SARC")
            {
                throw new Exception("Not a SARC file");
            }

            // Skip the header length
            Reader.Seek(2);

            // Check the byte order mark to see if this file is little endian
            if (Reader.ReadUInt16() == 0xFFFE)
            {
                // Set the endiannes to little
                Reader.ByteOrder = ByteOrder.LittleEndian;
            }

            // Check the file length
            if (Reader.ReadUInt32() != Reader.Length)
            {
                throw new Exception("SARC is possibly corrupt, invalid length");
            }

            // Read the beginning of data offset
            DataStartOfs = Reader.ReadUInt32();

            // Verify the version
            if (Reader.ReadUInt16() != 0x0100)
            {
                throw new Exception("Unsupported SARC version");
            }

            // Seek past the reserved area
            Reader.Seek(2);

            // Verify the SFAT magic numbers
            if (Reader.ReadString(4) != "SFAT")
            {
                throw new Exception("Could not find SFAT section");
            }

            // Skip the header length
            Reader.Seek(2);

            // Read the node count and hash key
            ushort nodeCount = Reader.ReadUInt16();
            uint hashKey = Reader.ReadUInt32();

            // Read every node
            List<SfatNode> nodes = new List<SfatNode>();
            for (ushort i = 0; i < nodeCount; i++)
            {
                // Read the node details
                uint hash = Reader.ReadUInt32();
                uint fileAttrs = Reader.ReadUInt32();
                uint nodeDataBeginOfs = Reader.ReadUInt32();
                uint nodeDataEndOfs = Reader.ReadUInt32();

                // Create a new SfatNode
                nodes.Add(new SfatNode()
                {
                    Hash = hash,
                    HasName = (fileAttrs & 0x01000000) == 0x01000000, // check for name flag
                    NameOfs = (int)(fileAttrs & 0x0000FFFF) * 4, // mask upper bits and multiply by 4
                    DataOfs = nodeDataBeginOfs,
                    DataLength = nodeDataEndOfs - nodeDataBeginOfs
                });
            }

            // Verify the SFNT magic numbers
            if (Reader.ReadString(4) != "SFNT")
            {
                throw new Exception("Could not find SFNT section");
            }

            // SKip header length and reserved area
            Reader.Seek(4);

            // Get the file name beginning offset
            long nameBeginOfs = Reader.Position;

            // Read each file using its SfatNode
            foreach (SfatNode node in nodes)
            {
                // Read the filename
                string filename;

                // Check if there is a name offset
                if (node.HasName)
                {
                    // Read the name at this position
                    using (Reader.TemporarySeek(nameBeginOfs + node.NameOfs, SeekOrigin.Begin))
                    {
                        filename = Reader.ReadString(BinaryStringFormat.ZeroTerminated);
                    }
                }
                else
                {
                    // Use the hash as the name
                    filename = node.Hash.ToString("X8") + ".bin";
                }

                // Add the file to the dictionary
                Files.Add(filename, node);
            }
        }

        private byte[] ReadFile(string str)
        {
            SfatNode node = Files[str];
            using (Reader.TemporarySeek(DataStartOfs + node.DataOfs, SeekOrigin.Begin))
            {
                return Reader.ReadBytes((int)node.DataLength);
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return Files.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            Reader.Dispose();
        }

    }
}