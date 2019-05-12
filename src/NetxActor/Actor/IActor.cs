using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Netx.Actor
{
    public interface IActorStatus
    {
        int Status { get; }
        int QueueCount { get; }
    }

    public interface IActor<R> : IActorStatus,IDisposable
    {
             
        void Action(long id, int cmd, OpenAccess access, params object[] args);
        ValueTask AsyncAction(long id, int cmd,OpenAccess access, params object[] args);
        ValueTask<R> AsyncFunc(long id, int cmd, OpenAccess access, params object[] args);      
    }
}
