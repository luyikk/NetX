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
        internal OpenAccess Access { get; }

        internal ActorResultAwaiter<T> Awaiter { get; }

        public ActorMessage(long id, int cmd, OpenAccess access, object[] args)
            :base(id,cmd,args)
        {
            this.Access = access;
            Awaiter = new ActorResultAwaiter<T>();
        }
    }
}
