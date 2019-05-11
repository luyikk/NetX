using System;
using System.Collections.Generic;
using System.Text;

namespace Netx
{
    public static class TimeHelper
    {
        public static long GetTime()
        {           
           return DateTime.UtcNow.Ticks;
        }

        public static DateTime GetTime(long time)
        {         
            return new DateTime(time, DateTimeKind.Utc).ToLocalTime();
        }
    }
}
