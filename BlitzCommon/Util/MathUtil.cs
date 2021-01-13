namespace BlitzCommon.Util
{
    public static class MathUtil
    {
        public static int RoundUpToMultiple(int num, int multiple)
        {
            int toNext = num % multiple;
            if (toNext == 0)
            {
                return num;
            }

            return num + multiple - toNext;
        }

    }
}
