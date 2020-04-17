using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Threading.Tasks.Sources.Copy;

namespace Netx.Actor
{
    public interface IActorMessage
    {
         long Id { get; }

         int Cmd { get; }       

         object[] Args { get; }

         long PushTime { get; set; }
    
         long CompleteTime { get; set; }
     
    }


    public abstract class ActorMessage : IActorMessage
    {
        public long Id { get; }

        public int Cmd { get; }

        public object[] Args { get; }

        public long PushTime { get; set; }

        public long CompleteTime { get; set; }

        internal OpenAccess Access { get; }

        protected ActorMessage(long id, int cmd, OpenAccess access, object[] args)
        {
            Id = id;
            Cmd = cmd;
            Args = args;
            PushTime = TimeHelper.GetTime();
            CompleteTime = 0;
            this.Access = access;
           
        }

        public abstract void Completed(object? result);
        public abstract void SetException(Exception error);
    }


    public class ActorMessage<T>: ActorMessage
    {
      
        internal ManualResetValueTaskSource<T> TaskSource { get; }

        internal ValueTask<T> Awaiter { get; }

        public ActorMessage(long id, int cmd, OpenAccess access, object[] args)
            :base(id,cmd,access,args)
        {                   
           
            TaskSource = new ManualResetValueTaskSource<T>();
            Awaiter = new ValueTask<T>(TaskSource, TaskSource.Version);
        }

        public override void Completed(object? result)
        {
            if (TaskSource.GetStatus(TaskSource.Version) == ValueTaskSourceStatus.Pending)
                TaskSource.SetResult((T)result!);
        }

        public override void SetException(Exception error)
        {
            if (TaskSource.GetStatus(TaskSource.Version) == ValueTaskSourceStatus.Pending)
                TaskSource.SetException(error);
        }

        public override string ToString()
        {
            return Cmd.ToString();
        }
    }
}
