using LibHac;
using LibHac.IO;
using Nintendo.Archive;
using Nintendo.Switch;
using System.Collections.Generic;
using System.IO;

namespace BlitzCommon.Resources.Source
{
    public class GameResourceRomSource : GameResourceSource
    {
        private NcaWrapper NcaWrapper;
        private Dictionary<string, Sarc> PackFiles;

        private GameResourceRomSource(Keyset keyset, string baseNca, string updateNca)
        {
            // Load the base and update NCAs
            NcaWrapper = new NcaWrapper(keyset, baseNca, updateNca);

            // Create dictionary to hold pack files
            PackFiles = new Dictionary<string, Sarc>();

            // Load the pack directory
            IDirectory packDirectory = NcaWrapper.Romfs.OpenDirectory("/Pack", OpenDirectoryMode.Files);

            // Read every directory entries
            foreach (DirectoryEntry directoryEntry in packDirectory.Read())
            {
                // Load the SARC
                Sarc sarc = new Sarc(GetRomFileFromRomfs(directoryEntry.FullPath));

                // Loop over every file
                foreach (string file in sarc)
                {
                    // Add this file to the pack files dictionary
                    PackFiles.Add("/" + file, sarc);
                }
            }
        }

        public static void Initialize(Keyset keyset, string baseNca, string updateNca)
        {
            Instance = new GameResourceRomSource(keyset, baseNca, updateNca);
        }

        public Stream GetRomFileFromRomfs(string romPath)
        {
            IFile file = NcaWrapper.Romfs.OpenFile(romPath, OpenMode.Read);
            return file.AsStream();
        }

        protected override Stream GetFileInternal(string path)
        {
            // Try to load from the packs first
            if (PackFiles.TryGetValue(path, out Sarc sarc))
            {
                string gamePath = path.StartsWith('/') ? path.Substring(1) : path;
                return new MemoryStream(sarc[gamePath]);
            }
            else
            {
               return GetRomFileFromRomfs(path);
            }
        }

    }
}
