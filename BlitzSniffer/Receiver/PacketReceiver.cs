using BlitzSniffer.Clone;
using NintendoNetcode.Pia;
using NintendoNetcode.Pia.Clone;
using NintendoNetcode.Pia.Clone.Content;
using NintendoNetcode.Pia.Clone.Element.Data;
using NintendoNetcode.Pia.Clone.Element.Data.Event;
using NintendoNetcode.Pia.Clone.Element.Data.Reliable;
using NintendoNetcode.Pia.Clone.Element.Data.Unreliable;
using NintendoNetcode.Pia.Lan.Content.Browse;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace BlitzSniffer.Receiver
{
    public abstract class PacketReceiver : IDisposable
    {
        static byte[] BlitzGameKey = { 0xee, 0x18, 0x2a, 0x63, 0xe2, 0x16, 0xcd, 0xb1, 0xf5, 0x1a, 0xd4, 0xbe, 0xd8, 0xcf, 0x65, 0x08 };

        protected ICaptureDevice Device
        {
            get;
            set;
        }

        private byte[] SessionKey
        {
            get;
            set;
        } = null;

        private IPAddress HostAddress
        {
            get;
            set;
        } = null;

        public virtual void Start()
        {
            Device.Filter = "ip and udp and (udp portrange 40000-49160 or udp port 30000)";
            Device.OnPacketArrival += OnPacketArrival;
            Device.StartCapture();

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        public virtual void Dispose()
        {
            Device.Close();
        }

        protected virtual void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            Packet packet = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);

            UdpPacket udpPacket = packet.Extract<UdpPacket>();
            IPPacket ipPacket = packet.Extract<IPPacket>();

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
                            throw new SnifferException("Session key mismatch - are there two sessions running at once?");
                        }

                        SessionKey = newKey;
                        HostAddress = ipPacket.SourceAddress;
                    }
                }
                else
                {
                    if (SessionKey == null)
                    {
                        Console.WriteLine($"WARN: skipping packet with length {udpPacket.PayloadData.Length} because there is no session key yet");
                        return;
                    }

                    byte[] address = ipPacket.SourceAddress.GetAddressBytes();
                    PiaPacket piaPacket = new PiaPacket(reader, SessionKey, BitConverter.ToUInt32(address));

                    foreach (PiaMessage message in piaPacket.Messages)
                    {
                        if (message.ProtocolId == PiaProtocol.Clone)
                        {
                            CloneMessage cloneMessage = message as CloneMessage;
                            CloneContentData cloneContentData = cloneMessage.Content as CloneContentData;

                            if (cloneContentData == null || !CloneHolder.Instance.IsCloneRegistered(cloneContentData.CloneId))
                            {
                                continue;
                            }

                            foreach (CloneElementData cloneElementData in cloneContentData.ElementData)
                            {
                                Type type = cloneElementData.GetType();
                                if (type == typeof(CloneElementDataEventData))
                                {
                                    CloneHolder.Instance.UpdateElementInClone(cloneContentData.CloneId, cloneElementData.Id, (cloneElementData as CloneElementDataEventData).Data);
                                }
                                else if (type == typeof(CloneElementDataReliableData))
                                {
                                    CloneHolder.Instance.UpdateElementInClone(cloneContentData.CloneId, cloneElementData.Id, (cloneElementData as CloneElementDataReliableData).Data);
                                }
                                else if (type == typeof(CloneElementDataUnreliable))
                                {
                                    CloneHolder.Instance.UpdateElementInClone(cloneContentData.CloneId, cloneElementData.Id, (cloneElementData as CloneElementDataUnreliable).Data);
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
