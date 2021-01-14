using BlitzCommon.Nintendo.MessageStudio;
using BlitzCommon.Resources.Source;
using LibHac;
using Nintendo;
using Nintendo.Archive;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace LocalizationExporter
{
    class Program
    {
        private static readonly string LOG_FORMAT = "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";

        private static Sarc CommonMsgSarc;
        private static Sarc LayoutMsgSarc;
        private static Dictionary<string, string> LocalizedWeaponsDict = new Dictionary<string, string>();
        private static Dictionary<string, string> LocalizedGearDict = new Dictionary<string, string>();
        private static Dictionary<string, string> LocalizedGearSkillsDict = new Dictionary<string, string>();
        private static Dictionary<string, string> LocalizedCoopEnemiesDict = new Dictionary<string, string>();
        private static Dictionary<string, string> LocalizedStagesDict = new Dictionary<string, string>();
        private static Dictionary<string, string> LocalizedSignalsDict = new Dictionary<string, string>();

        static void Main(string baseNca, string updateNca, string language = "USen")
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: LOG_FORMAT)
                .CreateLogger();

            ILogger logContext = Log.ForContext(Constants.SourceContextPropertyName, "Program");

            logContext.Information("LocalizationExporter {Version}", ThisAssembly.AssemblyFileVersion);

            // Just assume the keys are in the default location
            string keyDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".switch");
            Keyset keyset = ExternalKeys.ReadKeyFile(Path.Combine(keyDirectory, "prod.keys"), Path.Combine(keyDirectory, "title.keys"));

            logContext.Information("Loaded keys");

            GameResourceRomSource.Initialize(keyset, baseNca, updateNca);

            logContext.Information("Loaded ROM");

            CommonMsgSarc = new Sarc(GameResourceSource.Instance.GetFile($"/Message/CommonMsg_{language}.release.szs"));
            LayoutMsgSarc = new Sarc(GameResourceSource.Instance.GetFile($"/Message/LayoutMsg_{language}.release.szs"));

            logContext.Information("Loaded CommonMsg from ROM");

            LoadWeaponInfo("Main", "Wst", true);
            LoadWeaponInfo("Sub", "Wsb", false);
            LoadWeaponInfo("Special", "Wsp", false);

            logContext.Information("Fetched weapon info");

            LoadGearInfo("Head", "Hed");
            LoadGearInfo("Clothes", "Clt");
            LoadGearInfo("Shoes", "Shs");

            logContext.Information("Fetched gear info");

            LoadGearSkills();

            logContext.Information("Fetched gear skills");

            LoadCoopEnemies();

            logContext.Information("Fetched Coop enemies");

            LoadStages();

            logContext.Information("Fetched stages");

            LoadSignals();

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };

            string weaponsJson = JsonSerializer.Serialize(LocalizedWeaponsDict, options);
            string gearJson = JsonSerializer.Serialize(LocalizedGearDict, options);
            string gearSkillsJson = JsonSerializer.Serialize(LocalizedGearSkillsDict, options);
            string coopEnemiesJson = JsonSerializer.Serialize(LocalizedCoopEnemiesDict, options);
            string stagesJson = JsonSerializer.Serialize(LocalizedStagesDict, options);
            string signalsJson = JsonSerializer.Serialize(LocalizedSignalsDict, options);

            string outputDirectory = $"{language}";

            Directory.CreateDirectory(outputDirectory);

            File.WriteAllText(Path.Combine(outputDirectory, "weapons.json"), weaponsJson);
            File.WriteAllText(Path.Combine(outputDirectory, "gear.json"), gearJson);
            File.WriteAllText(Path.Combine(outputDirectory, "gear_skills.json"), gearSkillsJson);
            File.WriteAllText(Path.Combine(outputDirectory, "coop_enemies.json"), coopEnemiesJson);
            File.WriteAllText(Path.Combine(outputDirectory, "stages.json"), stagesJson);
            File.WriteAllText(Path.Combine(outputDirectory, "signals.json"), signalsJson);

            logContext.Information("Serialized to JSON");

            logContext.Information("Done!");
        }

        private static void LoadWeaponInfo(string type, string namePrefix, bool appendRelease)
        {
            string bymlPath;
            if (appendRelease)
            {
                bymlPath = $"/Mush/WeaponInfo_{type}.release.byml";
            }
            else
            {
                bymlPath = $"/Mush/WeaponInfo_{type}.byml";
            }

            Msbt msbt = new Msbt(CommonMsgSarc[$"WeaponName_{type}.msbt"]);

            using Stream infoStream = GameResourceSource.Instance.GetFile(bymlPath);
            dynamic infoByml = ByamlLoader.LoadByamlDynamic(infoStream);

            foreach (Dictionary<string, dynamic> info in infoByml)
            {
                string name = info["Name"];

                if (msbt.ContainsKey(name))
                {
                    LocalizedWeaponsDict.Add($"{namePrefix}_{name}", msbt.Get(name));
                }
            }
        }

        private static void LoadGearInfo(string type, string namePrefix)
        {
            string bymlPath = $"/Mush/GearInfo_{type}.release.byml";

            Msbt msbt = new Msbt(CommonMsgSarc[$"GearName_{type}.msbt"]);

            using Stream infoStream = GameResourceSource.Instance.GetFile(bymlPath);
            dynamic infoByml = ByamlLoader.LoadByamlDynamic(infoStream);

            foreach (Dictionary<string, dynamic> info in infoByml)
            {
                string name = info["Name"];

                if (msbt.ContainsKey(name))
                {
                    LocalizedGearDict.Add($"{namePrefix}_{name}", msbt.Get(name));
                }
            }
        }

        private static void LoadGearSkills()
        {
            Msbt msbt = new Msbt(CommonMsgSarc["GearSkillName.msbt"]);

            foreach (string key in msbt.Keys)
            {
                LocalizedGearSkillsDict.Add(key, msbt.Get(key));
            }
        }

        private static void LoadCoopEnemies()
        {
            Msbt msbt = new Msbt(CommonMsgSarc["CoopEnemy.msbt"]);

            foreach (string key in msbt.Keys)
            {
                LocalizedCoopEnemiesDict.Add(key, msbt.Get(key));
            }
        }

        private static void LoadStages()
        {
            using Stream mapInfoStream = GameResourceSource.Instance.GetFile("/Mush/MapInfo.release.byml");
            dynamic mapInfo = ByamlLoader.LoadByamlDynamic(mapInfoStream);

            Msbt versusMsbt = new Msbt(CommonMsgSarc["VSStageName.msbt"]);
            Msbt coopMsbt = new Msbt(CommonMsgSarc["CoopStageName.msbt"]);

            foreach (Dictionary<string, dynamic> map in mapInfo)
            {
                int id = map["Id"];
                string mapFileName = map["MapFileName"];

                string name;
                Msbt msbt;

                if (id >= 0 && id <= 200)
                {
                    msbt = versusMsbt;
                    name = mapFileName.Substring(4).Replace("_Vss", null);
                }
                else if (id >= 5000 && id <= 5100)
                {
                    msbt = coopMsbt;
                    name = mapFileName.Substring(4).Replace("_Cop", null);
                }
                else
                {
                    continue;
                }

                if (!msbt.ContainsKey(name))
                {
                    continue;
                }

                LocalizedStagesDict[mapFileName] = msbt.Get(name);
            }
        }

        private static void LoadSignals()
        {
            Msbt signalMsbt = new Msbt(LayoutMsgSarc["VS_ShoutBalloon_00.msbt"]);

            foreach (string key in signalMsbt.Keys)
            {
                string newKey;
                switch (key)
                {
                    case "000":
                        newKey = "Booyah";
                        break;
                    case "001":
                        newKey = "ThisWay";
                        break;
                    case "002":
                        newKey = "Help";
                        break;
                    case "003":
                        newKey = "Ouch";
                        break;
                    default:
                        throw new Exception("Unknown signal");
                }

                LocalizedSignalsDict[newKey] = signalMsbt.Get(key);
            }
        }

    }
}
