using System.Collections.Generic;

namespace BlitzSniffer.Clone
{
    class CloneHolder
    {
        private static CloneHolder _Instance = null;

        public static CloneHolder Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new CloneHolder();
                }

                return _Instance;
            }
        }

        public Dictionary<uint, Dictionary<uint, byte[]>> Clones
        {
            get;
            set;
        }

        public uint Clock
        {
            get;
            private set;
        }

        public delegate void CloneChangedEventHandler(object sender, CloneChangedEventArgs args);
        public event CloneChangedEventHandler CloneChanged;

        public delegate void ClockChangedEventHandler(object sender, ClockChangedEventArgs args);
        public event ClockChangedEventHandler ClockChanged;

        private CloneHolder()
        {
            Clones = new Dictionary<uint, Dictionary<uint, byte[]>>();
        }

        public void RegisterClone(uint id)
        {
            if (IsCloneRegistered(id))
            {
                return;
            }

            Clones[id] = new Dictionary<uint, byte[]>();
        }

        public bool IsCloneRegistered(uint id)
        {
            return Clones.ContainsKey(id);
        }

        public Dictionary<uint, byte[]> GetClone(uint id)
        {
            if (Clones.TryGetValue(id, out Dictionary<uint, byte[]> cloneData))
            {
                return cloneData;
            }

            throw new SnifferException($"Clone {id} not found");
        }

        public void UpdateElementInClone(uint cloneId, uint elementId, byte[] data, ulong sourceId)
        {
            if (Clones.TryGetValue(cloneId, out Dictionary<uint, byte[]> cloneData))
            {
                cloneData[elementId] = data;

                CloneChanged(this, new CloneChangedEventArgs(cloneId, elementId, data, sourceId));
            }
            else
            {
                throw new SnifferException($"Clone {cloneId} not found");
            }
        }

        public void UpdateCloneClock(uint clock)
        {
            if (Clock < clock)
            {
                Clock = clock;

                ClockChanged(this, new ClockChangedEventArgs(clock));
            }
        }

    }
}
