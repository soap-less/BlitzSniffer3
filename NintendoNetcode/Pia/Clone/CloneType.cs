namespace NintendoNetcode.Pia.Clone
{
    public enum CloneType : byte
    {
        Send = 0x1,
        Receive = 0x2,
        Atomic = 0x3,
        Sequential = 0x4
    }
}
