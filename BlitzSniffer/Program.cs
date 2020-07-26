using BlitzSniffer.Config;
using BlitzSniffer.Receiver;
using BlitzSniffer.Resources.Source;
using BlitzSniffer.Tracker;
using BlitzSniffer.WebSocket;
using LibHac;
using SharpPcap;
using System;
using System.IO;

namespace BlitzSniffer
{
    class Program
    {
        static void Main(bool useRom = false, FileInfo replayFile = null, bool replayInRealTime = false, int realTimeStartOffset = 0)
        {
            SnifferConfig.Load();

            ICaptureDevice captureDevice = GetCaptureDevice();

            if (captureDevice == null && replayFile == null)
            {
                while (true)
                {
                    for (int i = 0; i < CaptureDeviceList.Instance.Count; i++)
                    {
                        Console.WriteLine($"Device #{i + 1}\n{CaptureDeviceList.Instance[i]}");
                    }

                    Console.Write("Enter the device number to set as default: ");

                    int deviceNumber;
                    if (int.TryParse(Console.ReadLine(), out int result))
                    {
                        deviceNumber = result - 1;
                    }
                    else
                    {
                        Console.WriteLine("\nInvalid selection.\n");
                        continue;
                    }

                    if (deviceNumber < 0 || deviceNumber >= CaptureDeviceList.Instance.Count)
                    {
                        Console.WriteLine("\nInvalid selection.\n");
                        continue;
                    }

                    captureDevice = CaptureDeviceList.Instance[deviceNumber];
                    SnifferConfig.Instance.DefaultDevice = captureDevice.Name;

                    break;
                }
            }

            SnifferConfig.Instance.Save();

            Console.Clear();

            if (useRom)
            {
                RomConfig romConfig = SnifferConfig.Instance.Rom;
                Keyset keyset = ExternalKeys.ReadKeyFile(romConfig.ProdKeys, romConfig.TitleKeys);
                GameResourceRomSource.Initialize(keyset, romConfig.BaseNca, romConfig.UpdateNca);
            }
            else
            {
                GameResourceSnifferArchiveSource.Initialize();
            }

            SnifferServer.Initialize();

            GameSession.Initialize();

            PacketReceiver packetReceiver;
            if (replayFile != null)
            {
                if (replayInRealTime)
                {
                    packetReceiver = new RealTimeReplayPacketReceiver(replayFile.FullName, realTimeStartOffset);
                }
                else
                {
                    packetReceiver = new ReplayPacketReceiver(replayFile.FullName);
                }
            }
            else
            {
                packetReceiver = new LivePacketReceiver(captureDevice);
            }

            packetReceiver.Start();

            Console.ReadLine();

            try
            {
                packetReceiver.Dispose();
            }
            catch (PlatformNotSupportedException)
            {
                // Forcefully exit - ICaptureDevice.Close() might throw an exception on Windows
                // "Thread abort not supported on this platform"
                Environment.Exit(0);
            }
        }

        static ICaptureDevice GetCaptureDevice()
        {
            if (SnifferConfig.Instance.DefaultDevice == "none")
            {
                return null;
            }

            foreach (ICaptureDevice device in CaptureDeviceList.Instance)
            {
                if (SnifferConfig.Instance.DefaultDevice == device.Name)
                {
                    return device;
                }
            }

            return null;
        }

    }
}
