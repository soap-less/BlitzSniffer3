namespace NintendoNetcode.Pia.Clone.Content
{
    public enum CloneContentType : byte
    {
        ClockRequest = 0x10,
        ClockReply = 0x20,
        Participate = 0x30,
        ExitAck = 0x40,
        CloneCommand = 0x80,
        ClockCloneCommand = 0x90,
        ClockAndCountCloneCommand = 0xA0,
        ClockAndParticipantCloneCommand = 0xB0,
        ClockAndCountAndParicipantCloneCommand = 0xC0,
        Data = 0xD0,
        DataWithDestIndex = 0xE0,
        DataWithDestBitmap = 0xF0
    }
}
