using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Service
{
    public class NetxOption
    {
        /// <summary>
        /// 多久检查一遍超时
        /// </summary>
        public int ClearCheckTime { get; set; } = 1000;

        /// <summary>
        /// 断线后Session清理时间
        /// </summary>
        public long ClearSessionTime { get; set; } = 300000; //5分钟

        /// <summary>
        /// 请求超时
        /// </summary>
        public long ClearRequestTime { get; set; } = -1; //-1等于不开启
    }
}
