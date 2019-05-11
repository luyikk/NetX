using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Netx.Actor
{
    public class ActorMessage
    {
        public long Id { get; }

        public int Cmd { get; }

        public object[] Args { get; }

        public long PushTime { get; set; }
    
        public long CompleteTime { get; set; }

        public ActorMessage(long id, int cmd, object[] args)
        {
            Id = id;
            Cmd = cmd;
            Args = args;
            PushTime = TimeHelper.GetTime();
        }
    }

    public class ActorMessage<T>: ActorMessage
    {

        internal ActorResultAwaiter<T> Awaiter { get; }

        public ActorMessage(long id, int cmd, object[] args)
            :base(id,cmd,args)
        {
            Awaiter = new ActorResultAwaiter<T>();
        }
    }
}
