using Ionic.Zlib;
using NintendoNetcode.Pia.Clone.Element.Data;
using NintendoNetcode.Pia.Clone.Element.Data.Event;
using NintendoNetcode.Pia.Clone.Element.Data.Reliable;
using NintendoNetcode.Pia.Clone.Element.Data.Unreliable;
using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.IO;

namespace NintendoNetcode.Pia.Clone.Content
{
    public abstract class CloneContentData : CloneContentMessage
    {
        public List<CloneElementData> ElementData
        {
            get;
            set;
        }

        protected CloneContentData(BinaryDataReader reader) : base(reader)
        {

        }

        protected void DeserializeData(BinaryDataReader reader)
        {
            bool isCompressed = (RawContentType & 0xF) >= 0x3;

            int dataLength = (int)(reader.Length - reader.Position);
            byte[] data = reader.ReadBytes(dataLength);

            Stream rawDataStream;
            if (isCompressed)
            {
                rawDataStream = new MemoryStream();
                using (ZlibStream zlibStream = new ZlibStream(rawDataStream, CompressionMode.Decompress, true))
                {
                    zlibStream.Write(data, 0, data.Length);
                    rawDataStream.Seek(0, SeekOrigin.Begin);
                }
            }
            else
            {
                rawDataStream = new MemoryStream(data);
            }

            ElementData = new List<CloneElementData>();

            using (BinaryDataReader innerReader = new BinaryDataReader(rawDataStream))
            {
                innerReader.ByteOrder = ByteOrder.BigEndian;

                while (innerReader.Position != innerReader.Length)
                {
                    Type classType;
                    using (innerReader.TemporarySeek())
                    {
                        int type = innerReader.ReadByte() & 0xF0;
                        switch (type)
                        {
                            case 0x10:
                                classType = typeof(CloneElementDataUnreliable);
                                break;
                            case 0x20:
                                innerReader.Seek(3, SeekOrigin.Current);

                                byte reliableType = innerReader.ReadByte();
                                switch (reliableType)
                                {
                                    case 0x1:
                                    case 0x4:
                                        classType = typeof(CloneElementDataReliableNoData);
                                        break;
                                    case 0x2:
                                    case 0x3:
                                        classType = typeof(CloneElementDataReliableData);
                                        break;
                                    case 0x5:
                                    case 0x6:
                                        classType = typeof(CloneElementDataReliableAck);
                                        break;
                                    default:
                                        throw new PiaException("Unknown reliable type");
                                }

                                break;
                            case 0x30:
                                innerReader.Seek(3, SeekOrigin.Current);

                                byte eventType = innerReader.ReadByte();
                                switch (eventType)
                                {
                                    case 0x1:
                                    case 0x4:
                                    case 0x5:
                                        classType = typeof(CloneElementDataEventCommand);
                                        break;
                                    case 0x2:
                                        classType = typeof(CloneElementDataEventInitialAck);
                                        break;
                                    case 0x3:
                                        classType = typeof(CloneElementDataEventData);
                                        break;
                                    default:
                                        throw new PiaException("Unknown event type");
                                }

                                break;
                            default:
                                throw new PiaException("Unknown clone element type");
                        }
                    }

                    ElementData.Add((CloneElementData)Activator.CreateInstance(classType, innerReader));
                }
            }
        }

    }
}
