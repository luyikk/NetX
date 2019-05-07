using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Actor
{
    public static class TimeHelper
    {
        public static long GetTime()
        {           
           return DateTime.UtcNow.Ticks / 10000000;
        }

        public static DateTime GetTime(long time)
        {
            long Eticks = (long)(time * 10000000);
            return new DateTime(Eticks,DateTimeKind.Utc).ToLocalTime();
        }
    }
}
