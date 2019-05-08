using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Netx.Actor
{
    internal class LineByLineScheduler : ActorScheduler
    {
        public override Task Scheduler(Func<Task> action)
        {
            return action();
        }
    }


    internal class ThreadPoolScheduler : ActorScheduler
    {
        public override Task Scheduler(Func<Task> action)
        {
            return Task.Factory.StartNew(action);
        }
    }


    public abstract class ActorScheduler
    {
        
        public static ActorScheduler LineByLine { get => new LineByLineScheduler(); }
        public static ActorScheduler ThreadPool { get => new ThreadPoolScheduler(); }

        public abstract Task Scheduler(Func<Task> action);
    }
}
