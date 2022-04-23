using System;

namespace HttpSiraStatus.Util
{
    public static class Utility
    {
        public static long GetCurrentTime()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
