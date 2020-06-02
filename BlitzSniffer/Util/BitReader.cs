using Syroot.BinaryData;

namespace BlitzSniffer.Util
{
    public class BitReader
    {
        private BinaryDataReader InnerReader
        {
            get;
            set;
        }

        private long ReaderPosition
        {
            get;
            set;
        }

        public byte CurrentByte
        {
            get;
            set;
        }

        public int BitPosition
        {
            get;
            set;
        } = 8;

        public BitReader(BinaryDataReader reader)
        {
            InnerReader = reader;
        }

        public bool ReadBit()
        {
            if (BitPosition >= 8)
            {
                CurrentByte = InnerReader.ReadByte();
                ReaderPosition = InnerReader.Position;
                BitPosition = 0;
            }

            if (ReaderPosition != InnerReader.Position)
            {
                throw new SnifferException("Reader position changed while reading bits");
            }

            return (CurrentByte & (1 << BitPosition++)) > 0;
        }

        public void Seek(int bits)
        {
            int bytesToSeek = bits / 8;
            int bitsToSeek = bits % 8;

            BitPosition += bitsToSeek;
            if (BitPosition >= 8)
            {
                bytesToSeek++;
                BitPosition -= 8;
            }

            InnerReader.Seek(bytesToSeek - 1);
            CurrentByte = InnerReader.ReadByte();
            ReaderPosition = InnerReader.Position;
        }

        public uint ReadVariableBits(int bits)
        {
            uint u = 0;
            for (int i = 0; i < bits; i++)
            {
                int bit = ReadBit() ? 1 : 0;
                u |= (uint)(bit << i);
            }

            return u;
        }

        public byte ReadByte()
        {
            return (byte)ReadVariableBits(8);
        }

        public ushort ReadUInt16()
        {
            byte one = ReadByte();
            byte two = ReadByte();
            return (ushort)(one | two << 8);
        }

        public uint ReadUInt32()
        {
            byte one = ReadByte();
            byte two = ReadByte();
            byte three = ReadByte();
            byte four = ReadByte();

            return (uint)(one | two << 8 | three << 16 | four << 24);
        }

    }
}
