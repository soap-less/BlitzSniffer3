using Blitz;
using Nintendo;
using Nintendo.Archive;
using System.Collections.Generic;
using System.IO;

namespace BlitzSniffer.Resources
{
    class StageResource
    {
        private static StageResource _Instance;
        
        public static StageResource Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new StageResource();
                }

                return _Instance;
            }
        }

        private Dictionary<int, string> Stages = new Dictionary<int, string>();

        public StageResource()
        {
            dynamic mapInfo;
            using (Stream mapInfoStream = RomResourceLoader.Instance.GetRomFile("/Mush/MapInfo.release.byml"))
            {
                mapInfo = ByamlLoader.LoadByamlDynamic(mapInfoStream);
            }

            foreach (Dictionary<string, dynamic> map in mapInfo)
            {
                int id = map["Id"];

                if (id == -1)
                {
                    continue;
                }

                Stages.Add(id, map["MapFileName"]);
            }
        }

        public dynamic LoadStageForId(int id)
        {
            using (Stream stream = RomResourceLoader.Instance.GetRomFile($"/Map/{Stages[id]}.szs"))
            {
                Sarc sarc = new Sarc(stream);
                return ByamlLoader.LoadByamlDynamic(sarc[$"{Stages[id]}.byaml"]);
            }
        }

    }
}
