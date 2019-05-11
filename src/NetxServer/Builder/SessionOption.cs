using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Service
{
    public class SessionOption
    {
        public int ClecrCheckTime { get; set; } = 1000;

        public long ChecrOutTime { get; set; } = 10000; //5分钟
    }
}
