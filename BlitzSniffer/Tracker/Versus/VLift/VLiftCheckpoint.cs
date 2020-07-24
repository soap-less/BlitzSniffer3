using System.Diagnostics;

namespace BlitzSniffer.Tracker.Versus.VLift
{
    public class VLiftCheckpoint
    {
        public uint BaseHp
        {
            get;
            private set;
        }

        public uint _Hp;

        public uint Hp
        {
            get
            {
                return _Hp;
            }
            set
            {
                Debug.Assert(BaseHp >= value, "Checkpoint HP cannot exceed limit");

                _Hp = value;

                if (BestHp > _Hp)
                {
                    BestHp = _Hp;
                }

                if (BestHp <= 0)
                {
                    BaseHp = 0;
                }
            }
        }

        public uint BestHp
        {
            get;
            private set;
        }

        public VLiftCheckpoint(uint hp)
        {
            BaseHp = hp;
            BestHp = BaseHp;
            Hp = BaseHp;
        }

    }
}
