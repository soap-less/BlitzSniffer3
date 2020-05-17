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

        public byte ReadByte()
        {
            byte b = 0;
            for (int i = 0; i < 8; i++)
            {
                int bit = ReadBit() ? 1 : 0;
                b = (byte)(b | (bit << i));
            }

            return b;
        }

        public ushort ReadUInt16()
        {
            byte one = ReadByte();
            byte two = ReadByte();
            return (ushort)(one | two << 8);
        }

    }
}
