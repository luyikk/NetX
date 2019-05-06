using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Netx.Actor
{
    public class SingleActorItem<T>
    {
        public long Id { get; }
    
        public int Cmd { get; }

        public object[] Args { get; }

        public long PushTime { get; }

        public ActorResultAwaiter<T> Awaiter { get; }

        public SingleActorItem(long id,int cmd,object[] args)
        {
            Id = id;           
            Cmd = cmd;
            Args = args;
            Awaiter = new ActorResultAwaiter<T>();
            PushTime = TimeHelper.GetTime();
        }
    }
}
