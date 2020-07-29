using BlitzSniffer.Clone;
using Nintendo.Sead;
using Syroot.BinaryData;
using System.IO;

namespace BlitzSniffer.Tracker.Versus
{
    abstract class GachiVersusGameStateTracker : VersusGameStateTracker
    {
        public uint AlphaScore
        {
            get;
            protected set;
        }

        public uint BravoScore
        {
            get;
            protected set;
        }

        public bool InOvertime
        {
            get;
            private set;
        }

        public GachiVersusGameStateTracker(ushort stage, Color4f alpha, Color4f bravo) : base(stage, alpha, bravo)
        {
            AlphaScore = 0;
            BravoScore = 0;

            CloneHolder holder = CloneHolder.Instance;
            holder.RegisterClone(100);
            holder.CloneChanged += HandleSystemEventInternal;
        }

        public override void Dispose()
        {
            CloneHolder holder = CloneHolder.Instance;
            holder.CloneChanged -= HandleSystemEventInternal;
        }

        private void HandleSystemEventInternal(object sender, CloneChangedEventArgs args)
        {
            if (args.CloneId != 100 || args.ElementId != 1)
            {
                return;
            }

            using (MemoryStream stream = new MemoryStream(args.Data))
            using (BinaryDataReader reader = new BinaryDataReader(stream))
            {
                reader.ByteOrder = ByteOrder.LittleEndian;

                uint eventType = reader.ReadUInt32();
                HandleSystemEvent(eventType, reader);
            }
        }

        protected abstract void HandleSystemEvent(uint eventType, BinaryDataReader reader);

    }
}
