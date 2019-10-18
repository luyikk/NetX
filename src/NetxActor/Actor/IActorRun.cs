using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Netx.Actor
{
    public interface IActorRun: IActorGet, INetxBuildInterface,IDisposable
    {
        ConcurrentDictionary<int, Actor> ActorCollect { get; }
        event EventHandler<IActorMessage> CompletedEvent;
        ValueTask AsyncAction(long id, int cmd, OpenAccess access, params object[] args);      
        ValueTask<T> CallFunc<T>(long id, int cmd, OpenAccess access, params object[] args);       
        MethodRegister? GetCmdService(int cmd);
        void SyncAction(long id, int cmd, OpenAccess access, params object[] args);
    }
}