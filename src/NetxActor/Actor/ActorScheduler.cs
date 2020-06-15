using System;
using System.Threading.Tasks;

namespace Netx.Actor
{
    internal class LineByLineScheduler : ActorScheduler
    {
        public override Task Scheduler(Func<Task> action) => action();

    }


    internal class TaskScheduler : ActorScheduler
    {
        public override Task Scheduler(Func<Task> action) => Task.Factory.StartNew(action, TaskCreationOptions.DenyChildAttach);


    }

    internal class TaskRunScheduler : ActorScheduler
    {
        public override Task Scheduler(Func<Task> action) => Task.Run(action);

    }


    public abstract class ActorScheduler
    {

        public static ActorScheduler LineByLine { get => new LineByLineScheduler(); }
        public static ActorScheduler TaskFactory { get => new TaskScheduler(); }
        public static ActorScheduler TaskRun { get => new TaskRunScheduler(); }

        public abstract Task Scheduler(Func<Task> action);
    }
}
