using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Actor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ActorOptionAttribute : Attribute
    {
        /// <summary>
        /// 限制最大队列的数量
        /// </summary>
        public long MaxQueueCount { get; }

        /// <summary>
        /// 设置
        /// </summary>
        /// <param name="maxQueueCount">限制最大队列的数量</param>
        public ActorOptionAttribute(long maxQueueCount=0)
        {
            MaxQueueCount = maxQueueCount;
        }

    }
}
