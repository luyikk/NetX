using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Netx.Actor
{

    public class ActorRun<R> : ActorRunFodyInstance, IActorGet, IDisposable where R:class
    {

        private readonly Lazy<ConcurrentDictionary<int, Actor<R>>> actorCollect;

        public ConcurrentDictionary<int, Actor<R>> ActorCollect { get => actorCollect.Value; }

        public event EventHandler<IActorMessage> EventSourcing;

        public ActorRun(IServiceProvider container)
            : base(container)
        {
            actorCollect = new Lazy<ConcurrentDictionary<int, Actor<R>>>();
            Load();

            if(ActorCollect.Count>0)
                Task.Factory.StartNew(SleepingHandler);
        }

        private async void SleepingHandler()
        {
            while(true)
            {
                await Task.Delay(1000);

                foreach (var item in ActorCollect.Values)
                {
                    if (item.IsNeedSleep)
                    {
                        try
                        {
                            await item.AsyncAction(-1, Actor<R>.SleepCmd, OpenAccess.Private, null);
                        }
                        catch (Exception er)
                        {
                            Log.Error(er);
                        }
                    }
                }
            }
        }

        private void Load()
        {
            foreach (var controller in Container.GetServices<ActorController>())
            {
                var actor = new Actor<R>(Container,this,ActorScheduler,controller);
                actor.EventSourcing += Actor_CompletedEvent;
                foreach (int cmd in actor.CmdDict.Keys)
                    ActorCollect.AddOrUpdate(cmd, actor, (a, b) => actor);
            }
        }

        private void Actor_CompletedEvent(object sender, IActorMessage e)
        {
            EventSourcing?.Invoke(sender, e);           
        }

        public MethodRegister GetCmdService(int cmd)
        {
            if (ActorCollect.ContainsKey(cmd))            
                return ActorCollect[cmd].CmdDict[cmd];            
            else
                return null;

        }

        public void SyncAction(long id, int cmd,OpenAccess access, params object[] args)
        {
            if (ActorCollect.ContainsKey(cmd))
                ActorCollect[cmd].Action(id, cmd, access, args);
            else
                Log.Error($"not find actor service cmd:{cmd}");

        }

        public ValueTask AsyncAction(long id, int cmd, OpenAccess access, params object[] args)
        {
            if (ActorCollect.TryGetValue(cmd, out Actor<R> m))
                return m.AsyncAction(id, cmd, access, args);
            else
                throw new NetxException($"not find actor service cmd:{cmd}", ErrorType.ActorErr);
        }

        public  ValueTask<R> CallFunc(long id, int cmd, OpenAccess access, params object[] args)
        {
            if (ActorCollect.TryGetValue(cmd,out Actor<R> m))
                return  m.AsyncFunc(id, cmd, access, args);
            else
                throw new NetxException($"not find actor service cmd:{cmd}", ErrorType.ActorErr);
        }
        


        public async override  Task<IResult> AsyncFunc(int cmdTag, params object[] args)
        {
            var Id = IdsManager.MakeId;

            var result = await this.CallFunc(Id, cmdTag, OpenAccess.Internal, args);

            switch (result)
            {
                case Result rs: return rs;
                default:
                    {
                        return new Result(result)
                        {
                            Id = Id
                        };
                    }
            }
        }

        public async override Task<T> AsyncFunc<T>(int cmdTag, params object[] args)
        {
           
            var result =(dynamic) await this.CallFunc(IdsManager.MakeId, cmdTag, OpenAccess.Internal, args);
            return (T)result;
        }

        public async override Task AsyncAction(int cmdTag, params object[] args)
        {           
            await AsyncAction(IdsManager.MakeId, cmdTag, OpenAccess.Internal, args);
        }

        public override void Action(int cmdTag, params object[] args)
        {
            this.SyncAction(-1, cmdTag, OpenAccess.Internal, args);
        }


        public void Dispose()
        {
            foreach (var item in ActorCollect)
            {
                item.Value.Dispose();
            }

            ActorCollect.Clear();
        }

     
    }

    public class ActorRun : ActorRun<dynamic>
    {
        public ActorRun(IServiceProvider container) : base(container)
        {
        }
    }
}
