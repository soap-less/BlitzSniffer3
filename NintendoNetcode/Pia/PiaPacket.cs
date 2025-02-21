﻿using BlitzCommon.Util;
using NintendoNetcode.Pia.Encryption;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace NintendoNetcode.Pia
{
    public class PiaPacket
    {
        public static readonly uint PACKET_MAGIC = 0x32ab9864;

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

        public PiaPacket(Stream stream, PiaEncryptionArgs encryptionArgs, bool deserializeMessages = true)
        {
            using (BinaryDataReader reader = new BinaryDataReader(stream, true))
            {
                Deserialize(reader, encryptionArgs, deserializeMessages);
            }
        }

        public PiaPacket(BinaryDataReader reader, PiaEncryptionArgs encryptionArgs, bool deserializeMessages = true)
        {
            Deserialize(reader, encryptionArgs, deserializeMessages);
        }

        private void Deserialize(BinaryDataReader reader, PiaEncryptionArgs encryptionArgs, bool deserializeMessages)
        {
            reader.ByteOrder = ByteOrder.BigEndian;

            if (reader.ReadUInt32() != PACKET_MAGIC)
            {
                throw new PiaException("Invalid packet magic number");
            }

            IsEncrypted = reader.ReadByte() != 0x1;
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

                AesGcm aesGcm = new AesGcm(encryptionArgs.SessionKey);
                aesGcm.Decrypt(encryptionArgs.GetNonce(ConnectionId, PacketNonce), ciphertext, Tag, plaintext);
            }
            else
            {
                plaintext = ciphertext;
            }

            if (deserializeMessages)
            {
                Messages = new List<PiaMessage>();

                using (MemoryStream memoryStream = new MemoryStream(plaintext))
                using (BinaryDataReader innerReader = new BinaryDataReader(memoryStream))
                {
                    innerReader.ByteOrder = ByteOrder.BigEndian;

                    while (innerReader.Position + 20 < innerReader.Length)
                    {
                        PiaProtocol protocol;
                        using (innerReader.TemporarySeek(19))
                        {
                            protocol = (PiaProtocol)innerReader.ReadByte();
                        }

                        Messages.Add((PiaMessage)Activator.CreateInstance(PiaMessage.PiaMessageForProtocol(protocol), innerReader));

                        long toSeek = MathUtil.RoundUpToMultiple((int)innerReader.Position, 4) - innerReader.Position;
                        innerReader.Seek(toSeek);
                    }
                }
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
