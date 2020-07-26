using System;
using System.IO;
using System.Text.Json;

namespace BlitzSniffer.Config
{
    public class SnifferConfig
    {
        public static SnifferConfig Instance
        {
            get;
            private set;
        }

        private static readonly string ConfigPath = "config.json";

        public RomConfig Rom
        {
            get;
            set;
        }

        public string DefaultDevice
        {
            get;
            set;
        }

        public SnifferConfig()
        {
            Rom = new RomConfig();
            DefaultDevice = "none";
        }

        public static void Load()
        {
            if (File.Exists(ConfigPath))
            {
                Instance = JsonSerializer.Deserialize<SnifferConfig>(File.ReadAllText(ConfigPath));
            }
            else
            {
                Instance = new SnifferConfig();
            }
        }

        public void Save()
        {
            File.WriteAllText(ConfigPath, JsonSerializer.Serialize<SnifferConfig>(this, new JsonSerializerOptions()
            {
                WriteIndented = true
            }));
        }

    }
}
