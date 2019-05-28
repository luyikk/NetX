using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Netx
{
    public static class TimeHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetTime()
        {           
           return DateTime.UtcNow.Ticks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime GetTime(long time)
        {         
            return new DateTime(time, DateTimeKind.Utc).ToLocalTime();
        }
    }
}
