using System;
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


    internal class TaskScheduler : ActorScheduler
    {
        public override Task Scheduler(Func<Task> action)
        {
            return Task.Factory.StartNew(action, TaskCreationOptions.DenyChildAttach);
        }
    }

    internal class TaskRunScheduler : ActorScheduler
    {
        public override Task Scheduler(Func<Task> action)
        {
            return Task.Run(action);
        }
    }


    public abstract class ActorScheduler
    {

        public static ActorScheduler LineByLine => new LineByLineScheduler();
        public static ActorScheduler TaskFactory => new TaskScheduler();
        public static ActorScheduler TaskRun => new TaskRunScheduler();

        public abstract Task Scheduler(Func<Task> action);
    }
}
