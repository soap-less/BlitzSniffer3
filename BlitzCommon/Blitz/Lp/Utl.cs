namespace Blitz.Lp
{
    public static class Utl
    {
        // Lp::Utl::convCloneClockToFrame(uint)
        public static uint CloneClockToGameFrame(uint clock)
        {
            float calcFrameRate = 1.0f; // Lp::Utl::getCalcFrameRate(void)
            float calcsPerSecond = 60.0f / calcFrameRate;

            float cloneClockPerSecond = 120.0f; // Cstm::EnlTask::getCloneClockPerSec()
            float clocksPerSecond = calcsPerSecond / cloneClockPerSecond;

            return (uint)(clock * clocksPerSecond);
        }

    }
}
