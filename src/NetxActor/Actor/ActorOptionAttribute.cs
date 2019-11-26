using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Actor
{
    public enum SchedulerType
    {
        None =0,
        LineByLine=1,
        TaskFactory=2,
        TaskRun=3
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ActorOptionAttribute : Attribute
    {
        /// <summary>
        /// 限制最大队列的数量
        /// </summary>
        public long MaxQueueCount { get; }

        /// <summary>
        /// 多少毫秒秒没有订单沉睡
        /// </summary>
        public int Ideltime { get;  }

        /// <summary>
        /// 指定调度器
        /// </summary>
        public SchedulerType SchedulerType { get; }

        /// <summary>
        /// Actor设置
        /// </summary>
        /// <param name="maxQueueCount">限制最大队列的数量</param>
        /// <param name="sleepTime">多少毫秒秒没有订单沉睡</param>
        public ActorOptionAttribute(long maxQueueCount = 0, int ideltime = 5000, SchedulerType schedulerType= SchedulerType.None)
        {
            MaxQueueCount = maxQueueCount;
            Ideltime = ideltime;
            SchedulerType = schedulerType;
        }

    }
}
