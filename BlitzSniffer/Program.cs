using BlitzSniffer.Config;
using BlitzSniffer.Receiver;
using BlitzSniffer.Resources.Source;
using BlitzSniffer.Tracker;
using BlitzSniffer.WebSocket;
using LibHac;
using Serilog;
using Serilog.Core;
using SharpPcap;
using SKM.V3;
using SKM.V3.Methods;
using SKM.V3.Models;
using System;
using System.IO;

namespace BlitzSniffer
{
    class Program
    {
        private static readonly string LOG_FORMAT = "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        private static string CRYPTOLENS_PUBLIC_KEY = "<RSAKeyValue><Modulus>kXe0NP7Dco5g85KOziWQT+oK21VkKwp+4XeR6GOTf46u2F3UwdFK3UYA1wXxIobbWoCpvX+7Yq/gGlV03IEqjzfxePwMXKd31EIFT7fez/hKz29YRD6A9pIJwqnHfJo8Xfje/6vxj83nvlvLXLgLutJs4tKK+hM43EAKy2NEs3mF/qeu88tPX3MMkrqrN0N2/I2tPnUgiMjV/pZ02wWhZSFnsfxhpcmwUI0mTYPcYa8317oG2BoXtNiS7wpurHygZPPRpcqc/BJjR7117N3IY7GIBa7qsBhcyzjr86m+Wt2s65kt3A5vI9jAjQ7cTIPIhzvWJCoeVOwTdjJSpjZsxw==</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
        private static string CRYPTOLENS_AUTH_TOKEN = "WyIyNTg2NDgiLCJueitsRVVmQWFpVWIwVHM5RzdtTjJkNjkxekxHR0czb2ROU2phNEVyIl0=";

        /// <summary>
        /// Sniffs Splatoon 2 LAN sessions.
        /// </summary>
        /// <param name="useRom">If a Splatoon 2 ROM should be used instead of the GameData file.</param>
        /// <param name="replayFile">A pcap file to replay.</param>
        /// <param name="replayInRealTime">If the replay file should be replayed in real-time.</param>
        /// <param name="realTimeStartOffset">When to fast-forward to in the replay file.</param>
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

#if !DEBUG
            KeyInfoResult keyResult = Key.Activate(token: CRYPTOLENS_AUTH_TOKEN, parameters: new ActivateModel()
            {
                Key = SnifferConfig.Instance.Key,
                ProductId = 8988,
                Sign = true,
                MachineCode = Helpers.GetMachineCodePI()
            });

            if (keyResult == null || keyResult.Result == ResultType.Error || !keyResult.LicenseKey.HasValidSignature(CRYPTOLENS_PUBLIC_KEY).IsValid())
            {
                Console.WriteLine("Please contact OatmealDome.");
                return;
            }
#endif

            Console.Clear();

            Directory.CreateDirectory("Logs");
            string dateTime = DateTime.Now.ToString("s").Replace(':', '_');
            string logFile = Path.Combine("Logs", $"{dateTime}.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Async(c => c.Console(outputTemplate: LOG_FORMAT))
                .WriteTo.Async(c => c.File(logFile, outputTemplate: LOG_FORMAT))
                .CreateLogger();

            ILogger localLogContext = Log.ForContext(Constants.SourceContextPropertyName, "Program");

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

            LocalLog.RegisterConsoleDebug();

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

                localLogContext.Information("Waiting for user to start replay");
                Console.ReadLine();
            }
            else
            {
                packetReceiver = new LivePacketReceiver(captureDevice);
            }

            Directory.CreateDirectory("PacketCaptures");
            string pcapDumpFile = Path.Combine("PacketCaptures", $"{dateTime}.pcap");

            packetReceiver.Start(replayFile == null ? pcapDumpFile : null);

            localLogContext.Information("This session's log files are filed under \"{DateTime}\".", dateTime);
            localLogContext.Information("Start up complete. Press any key to exit.");

            Console.ReadLine();

            Log.CloseAndFlush();

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
