using System;
using System.Threading.Tasks;

namespace Netx.Actor
{
    public interface IActorStatus
    {
        int Status { get; }
        int QueueCount { get; }
    }

    public interface IActor : IActorStatus, IDisposable
    {

        void Action(long id, int cmd, OpenAccess access, params object[] args);
        ValueTask AsyncAction(long id, int cmd, OpenAccess access, params object[] args);
        ValueTask<T> AsyncFunc<T>(long id, int cmd, OpenAccess access, params object[] args);
    }
}
