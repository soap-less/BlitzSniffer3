using Nintendo.Archive;
using System.IO;

namespace BlitzSniffer.Resources.Source
{
    class GameResourceSnifferArchiveSource : GameResourceSource
    {
        private readonly static string ArchivePath = "GameData.sarc";

        private Sarc SnifferArchive;

        private GameResourceSnifferArchiveSource()
        {
            SnifferArchive = new Sarc(File.OpenRead(ArchivePath));
        }

        public static void Initialize()
        {
            Instance = new GameResourceSnifferArchiveSource();
        }

        protected override Stream GetFileInternal(string path)
        {
            byte[] rawFile = SnifferArchive[path.StartsWith('/') ? path.Substring(1) : path];
            return new MemoryStream(rawFile);
        }

    }
}
