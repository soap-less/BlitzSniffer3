using BlitzSniffer.Clone;
using BlitzSniffer.Config;
using BlitzSniffer.Enl;
using BlitzSniffer.Searcher;
using NintendoNetcode.Enl;
using NintendoNetcode.Pia;
using NintendoNetcode.Pia.Clone;
using NintendoNetcode.Pia.Clone.Content;
using NintendoNetcode.Pia.Clone.Element.Data;
using NintendoNetcode.Pia.Clone.Element.Data.Event;
using NintendoNetcode.Pia.Clone.Element.Data.Reliable;
using NintendoNetcode.Pia.Clone.Element.Data.Unreliable;
using NintendoNetcode.Pia.Encryption;
using NintendoNetcode.Pia.Unreliable;
using PacketDotNet;
using Serilog;
using Serilog.Core;
using SharpPcap;
using SharpPcap.LibPcap;
using Syroot.BinaryData;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace BlitzSniffer.Receiver
{
    public abstract class PacketReceiver : IDisposable
    {
        private static readonly ILogger LogContext = Log.ForContext(Constants.SourceContextPropertyName, "PacketReceiver");

        protected PiaSessionType SessionType
        {
            get;
            set;
        }

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

        private uint GatheringId
        {
            get;
            set;
        }

        private CaptureFileWriterDevice WriterDevice
        {
            get;
            set;
        }

        private BlockingCollection<RawCapture> CaptureQueue
        {
            get;
            set;
        }

        private Thread DumperThread
        {
            get;
            set;
        }

        private CancellationTokenSource DumperCancellationTokenSource
        {
            get;
            set;
        }

        protected PacketReceiver(PiaSessionType sessionType)
        {
            SessionType = sessionType;
        }

        public virtual void Start(string outputFile = null)
        {
            if (outputFile != null)
            {
                WriterDevice = new CaptureFileWriterDevice(outputFile);
                WriterDevice.Open();

                CaptureQueue = new BlockingCollection<RawCapture>();
                DumperCancellationTokenSource = new CancellationTokenSource();
                DumperThread = new Thread(DumpPackets);
                DumperThread.Start();
            }

            SessionSearcher.Instance.SessionFound += SessionFound;

            if (SessionType == PiaSessionType.Lan)
            {
                Device.Filter = "ip and udp and (udp portrange 49150-49160 or udp port 30000)";
            }
            else
            {
                Device.Filter = $"ip and udp and ip host {SnifferConfig.Instance.Snicom.IpAddress}";
            }

            Device.OnPacketArrival += OnPacketArrival;
            Device.StartCapture();
        }

        public virtual void Dispose()
        {
            if (WriterDevice != null)
            {
                DumperCancellationTokenSource.Cancel();
                DumperThread.Join();

                WriterDevice.Close();
            }

            Device.Close();
        }

        private void SessionFound(object sender, SessionFoundArgs e)
        {
            if (e.FoundDataType == SessionFoundDataType.Key)
            {
                SessionKey = e.Data;
            }
            else if (e.FoundDataType == SessionFoundDataType.GatheringId)
            {
                GatheringId = BitConverter.ToUInt32(e.Data);
            }
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

                try
                {
                    if (udpPacket.DestinationPort != 30000)
                    {
                        using (reader.TemporarySeek())
                        {
                            if (reader.ReadUInt32() != PiaPacket.PACKET_MAGIC)
                            {
                                return;
                            }
                        }

                        if (SessionKey == null)
                        {
                            LogContext.Warning("Skipping packet with length {Length}, no session key", udpPacket.PayloadData.Length);
                            return;
                        }

                        if (SessionType == PiaSessionType.Inet && GatheringId == 0)
                        {
                            LogContext.Warning("Skipping packet with length {Length}, no gathering ID", udpPacket.PayloadData.Length);
                            return;
                        }

                        HandlePiaPacket(reader, ipPacket.SourceAddress.GetAddressBytes());
                    }
                }
                catch (Exception ex) when (!Debugger.IsAttached)
                {
                    LogContext.Error(ex, "Exception while processing packet");
                }
            }

            if (WriterDevice != null)
            {
                CaptureQueue.Add(e.Packet);
            }
        }

        private void HandlePiaPacket(BinaryDataReader reader, byte[] sourceAddress)
        {
            PiaEncryptionArgs encryptionArgs;
            if (SessionType == PiaSessionType.Lan)
            {
                encryptionArgs = new PiaLanEncryptionArgs(SessionKey, sourceAddress);
            }
            else if (SessionType == PiaSessionType.Inet)
            {
                encryptionArgs = new PiaInetEncryptionArgs(SessionKey, GatheringId);
            }
            else
            {
                LogContext.Error("LDN sessions not supported");
                return;
            }

            PiaPacket piaPacket;
            try
            {
                piaPacket = new PiaPacket(reader, encryptionArgs);
            }
            catch (CryptographicException)
            {
                // Just skip... We probably don't have the correct key yet or someone is running
                // another game/session on this network.
                return;
            }

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
                byte[] data;
                uint clock;

                switch (cloneElementData)
                {
                    case CloneElementDataEventData eventData:
                        data = eventData.Data;
                        clock = eventData.Clock;

                        break;
                    case CloneElementDataReliableData reliableData:
                        data = reliableData.Data;
                        clock = reliableData.Clock;
                        
                        break;
                    case CloneElementDataUnreliable unreliableData:
                        data = unreliableData.Data;
                        clock = unreliableData.Clock;
                        
                        break;
                    default:
                        continue;
                }

                CloneHolder.Instance.UpdateCloneClock(clock);
                CloneHolder.Instance.UpdateElementInClone(cloneContentData.CloneId, cloneElementData.Id, data);
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

        private void DumpPackets()
        {
            while (!DumperCancellationTokenSource.IsCancellationRequested)
            {
                while (CaptureQueue.TryTake(out RawCapture capture))
                {
                    WriterDevice.Write(capture);
                }
            }
        }

        public ICaptureDevice GetDevice()
        {
            return Device;
        }

    }
}
