using NintendoNetcode.Pia;
using NintendoNetcode.Pia.Encryption;
using NintendoNetcode.Pia.Lan.Content.Browse;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PcapDecryptor
{
    class Program
    {
        static byte[] BlitzGameKey = { 0xee, 0x18, 0x2a, 0x63, 0xe2, 0x16, 0xcd, 0xb1, 0xf5, 0x1a, 0xd4, 0xbe, 0xd8, 0xcf, 0x65, 0x08 };
        static List<RawCapture> Packets = new List<RawCapture>();
        static byte[] SessionKey = null;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("dotnet PcapDecryptor.dll <path to pcap>");
                return;
            }

            CaptureFileReaderDevice readerDevice = new CaptureFileReaderDevice(args[0]);
            readerDevice.OnPacketArrival += device_OnPacketArrival;
            readerDevice.Open();
            readerDevice.Capture();
            readerDevice.Close();

            string path = /*Path.Combine(Path.GetDirectoryName(args[0]), */Path.GetFileNameWithoutExtension(args[0]) + "-decrypted.pcap";/*);*/
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            CaptureFileWriterDevice writerDevice = new CaptureFileWriterDevice(path);
            writerDevice.Open();

            foreach (RawCapture packet in Packets)
            {
                writerDevice.Write(packet);
            }

            writerDevice.Close();
        }

        private static void device_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            Packet packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            IPPacket ipPacket = packet.Extract<IPPacket>();
            UdpPacket udpPacket = packet.Extract<UdpPacket>();

            if (udpPacket == null || ipPacket == null)
            {
                return;
            }

            if (udpPacket.DestinationPort != 30000 && !(udpPacket.DestinationPort <= 49160 && udpPacket.DestinationPort >= 40000))
            {
                return;
            }

            using (MemoryStream memoryStream = new MemoryStream(udpPacket.PayloadData))
            using (BinaryDataReader reader = new BinaryDataReader(memoryStream))
            {
                reader.ByteOrder = ByteOrder.BigEndian;

                if (udpPacket.DestinationPort == 30000)
                {
                    byte firstByte = reader.ReadByte();

                    if (firstByte == 0x1)
                    {
                        LanContentBrowseReply browseReply = new LanContentBrowseReply(reader);
                        byte[] newKey = PiaEncryptionUtil.GenerateLanSessionKey(browseReply.SessionInfo.SessionParam, BlitzGameKey);

                        if (SessionKey != null && !newKey.SequenceEqual(SessionKey))
                        {
                            Console.WriteLine("WARN: Key mismatch, using new key from now on");
                        }

                        SessionKey = newKey;
                    }
                }
                else if (udpPacket.DestinationPort != 30000)
                {
                    if (SessionKey == null)
                    {
                        Console.WriteLine($"WARN: skipping pack with length {udpPacket.PayloadData.Length} because there is no session key yet");
                        return;
                    }

                    using (reader.TemporarySeek())
                    {
                        if (reader.ReadUInt32() != 0x32ab9864)
                        {
                            return;
                        }
                    }

                    byte[] address = ipPacket.SourceAddress.GetAddressBytes();
                    try
                    {
                        PiaEncryptionArgs encryptionArgs = new PiaLanEncryptionArgs(SessionKey, ipPacket.SourceAddress.GetAddressBytes());
                        PiaPacket piaPacket = new PiaPacket(reader, encryptionArgs, false);
                        piaPacket.IsEncrypted = false;
                        udpPacket.PayloadData = piaPacket.Serialize();
                    }
                    catch (CryptographicException)
                    {
                        Console.WriteLine($"WARN: decryption failure on packet with length {udpPacket.PayloadData.Length}");
                        return;
                    }
                }
            }
            RawCapture cap = new RawCapture(e.Packet.LinkLayerType, e.Packet.Timeval, packet.Bytes);
            Packets.Add(cap);
        }

    }
}
