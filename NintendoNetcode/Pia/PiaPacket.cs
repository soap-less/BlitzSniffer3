using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace NintendoNetcode.Pia
{
    public class PiaPacket
    {
        public bool IsEncrypted
        {
            get;
            set;
        }

        public byte ConnectionId
        {
            get;
            set;
        }

        public ushort PacketId
        {
            get;
            set;
        }

        public ushort SessionTimer
        {
            get;
            set;
        }

        public ushort RttTimer
        {
            get;
            set;
        }

        public byte[] PacketNonce
        {
            get;
            set;
        }

        public byte[] Tag
        {
            get;
            set;
        }

        public List<PiaMessage> Messages
        {
            get;
            set;
        }

        private byte[] Data
        {
            get;
            set;
        } = null;

        public PiaPacket(Stream stream, byte[] sessionKey, uint sourceIp, bool deserializeMessages = true)
        {
            using (BinaryDataReader reader = new BinaryDataReader(stream, true))
            {
                Deserialize(reader, sessionKey, sourceIp, deserializeMessages);
            }
        }

        public PiaPacket(BinaryDataReader reader, byte[] sessionKey, uint sourceIp, bool deserializeMessages = true)
        {
            Deserialize(reader, sessionKey, sourceIp, deserializeMessages);
        }

        private void Deserialize(BinaryDataReader reader, byte[] sessionKey, uint sourceIp, bool deserializeMessages)
        {
            reader.ByteOrder = ByteOrder.BigEndian;

            if (reader.ReadUInt32() != 0x32ab9864)
            {
                throw new PiaException("Invalid packet magic number");
            }

            IsEncrypted = reader.ReadByte() == 0x2;
            ConnectionId = reader.ReadByte();
            PacketId = reader.ReadUInt16();
            SessionTimer = reader.ReadUInt16();
            RttTimer = reader.ReadUInt16();
            PacketNonce = reader.ReadBytes(8);
            Tag = reader.ReadBytes(16);

            byte[] ciphertext = reader.ReadBytes((int)(reader.Length - reader.Position));

            byte[] plaintext;
            if (IsEncrypted)
            {
                plaintext = new byte[ciphertext.Length];

                byte[] nonce = new byte[12];
                Array.Copy(BitConverter.GetBytes(sourceIp), nonce, 4);
                Array.Copy(PacketNonce, 0, nonce, 4, 8);
                nonce[4] = ConnectionId;

                AesGcm aesGcm = new AesGcm(sessionKey);
                aesGcm.Decrypt(nonce, ciphertext, Tag, plaintext);
            }
            else
            {
                plaintext = ciphertext;
            }

            if (deserializeMessages)
            {
                // TODO
            }
            else
            {
                Data = plaintext;
            }
        }

        public byte[] Serialize()
        {
            if (Data == null)
            {
                throw new PiaException("Message serialization unsupported");
            }

            if (IsEncrypted)
            {
                throw new PiaException("Encrypted packet serialization unsupported");
            }

            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryDataWriter writer = new BinaryDataWriter(memoryStream))
            {
                writer.ByteOrder = ByteOrder.BigEndian;

                writer.Write(0x32ab9864);
                writer.Write((byte)(IsEncrypted ? 0x2 : 0x1));
                writer.Write(ConnectionId);
                writer.Write(PacketId);
                writer.Write(SessionTimer);
                writer.Write(RttTimer);
                writer.Seek(8);
                writer.Seek(16);
                writer.Write(Data);

                return memoryStream.ToArray();
            }
        }

    }
}
