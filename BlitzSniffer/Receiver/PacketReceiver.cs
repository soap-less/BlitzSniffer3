using BlitzSniffer.Clone;
using BlitzSniffer.Enl;
using NintendoNetcode.Enl;
using NintendoNetcode.Pia;
using NintendoNetcode.Pia.Clone;
using NintendoNetcode.Pia.Clone.Content;
using NintendoNetcode.Pia.Clone.Element.Data;
using NintendoNetcode.Pia.Clone.Element.Data.Event;
using NintendoNetcode.Pia.Clone.Element.Data.Reliable;
using NintendoNetcode.Pia.Clone.Element.Data.Unreliable;
using NintendoNetcode.Pia.Lan.Content.Browse;
using NintendoNetcode.Pia.Unreliable;
using PacketDotNet;
using Serilog;
using Serilog.Core;
using SharpPcap;
using Syroot.BinaryData;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace BlitzSniffer.Receiver
{
    public abstract class PacketReceiver : IDisposable
    {
        private static byte[] BlitzGameKey = { 0xee, 0x18, 0x2a, 0x63, 0xe2, 0x16, 0xcd, 0xb1, 0xf5, 0x1a, 0xd4, 0xbe, 0xd8, 0xcf, 0x65, 0x08 };

        private static readonly ILogger LogContext = Log.ForContext(Constants.SourceContextPropertyName, "PacketReceiver");

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
                    HandleLanSearchPacket(reader);
                }
                else
                {
                    if (SessionKey == null)
                    {
                        LogContext.Warning("Skipping packet with length {Length}, no session key", udpPacket.PayloadData.Length);
                        return;
                    }

                    HandlePiaPacket(reader, ipPacket.SourceAddress.GetAddressBytes());
                }
            }
        }

        private void HandleLanSearchPacket(BinaryDataReader reader)
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
            }
        }

        private void HandlePiaPacket(BinaryDataReader reader, byte[] sourceAddress)
        {
            PiaPacket piaPacket = new PiaPacket(reader, SessionKey, BitConverter.ToUInt32(sourceAddress));

            foreach (PiaMessage message in piaPacket.Messages)
            {
                if (message.ProtocolId == PiaProtocol.Clone)
                {
                    HandleCloneData(message as CloneMessage);
                }
                else if (message.ProtocolId == PiaProtocol.Unreliable && message.ProtocolPort == 0x01) // Enl
                {
                    HandleEnlPacket(message as UnreliableMessage);
                }
            }
        }

        private void HandleCloneData(CloneMessage cloneMessage)
        {
            CloneContentData cloneContentData = cloneMessage.Content as CloneContentData;

            if (cloneContentData == null || !CloneHolder.Instance.IsCloneRegistered(cloneContentData.CloneId))
            {
                return;
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

        private void HandleEnlPacket(UnreliableMessage unreliableMessage)
        {
            using (MemoryStream innerStream = new MemoryStream(unreliableMessage.Data))
            using (BinaryDataReader innerReader = new BinaryDataReader(innerStream))
            {
                innerReader.ByteOrder = ByteOrder.LittleEndian;

                EnlMessage enlMessage = new EnlMessage(innerReader, 10, 0);
                EnlHolder.Instance.EnlMessageReceived(enlMessage);
            }
        }

    }
}
