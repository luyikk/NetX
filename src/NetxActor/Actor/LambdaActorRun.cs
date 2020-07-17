using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Netx.Actor
{
    public class LambdaActorRun : ActorRunFodyInstance, IActorRun
    {
        private readonly Lazy<ConcurrentDictionary<int, Actor>> actorCollect;

        public ConcurrentDictionary<int, Actor> ActorCollect => actorCollect.Value;

        public event EventHandler<IActorMessage>? CompletedEvent;

        public string Key { get;  }

        public LambdaActorRun(string key,IServiceProvider container,LambdaController lambdaController)
            : base(container, container.GetRequiredService<ILoggerFactory>().CreateLogger("LambdaActorRun->"))
        {
            Key = key;
            actorCollect = new Lazy<ConcurrentDictionary<int, Actor>>();          
            var actor = new Actor(Container, this, ActorScheduler, lambdaController);
            actor.CompletedEvent += Actor_CompletedEvent;
            foreach (int cmd in actor.CmdDict.Keys)
                ActorCollect.AddOrUpdate(cmd, actor, (a, b) => actor);

            if (ActorCollect.Count > 0)
                Task.Factory.StartNew(SleepingHandler);

            foreach (var @event in container.GetServices<ActorEventBase>())
                this.CompletedEvent += @event.ActorEventCompleted;

        }

        private async void SleepingHandler()
        {
            while (true)
            {
                await Task.Delay(1000);

                foreach (var item in ActorCollect.Values)
                {
                    if (item.IsNeedSleep)
                    {
                        try
                        {
                            await item.AsyncAction(-1, Actor.SleepCmd, OpenAccess.Private, null!);
                        }
                        catch (Exception er)
                        {
                            Log.Error(er);
                        }
                    }
                }
            }
        }


        private void Actor_CompletedEvent(object sender, IActorMessage e)
        {
            CompletedEvent?.Invoke(sender, e);
        }

        public MethodRegister? GetCmdService(int cmd)
        {
            if (ActorCollect.ContainsKey(cmd))
                return ActorCollect[cmd].CmdDict[cmd];
            else
                return null;

        }

        public void SyncAction(long id, int cmd, OpenAccess access, params object[] args)
        {
            if (ActorCollect.ContainsKey(cmd))
                ActorCollect[cmd].Action(id, cmd, access, args);
            else
                Log.ErrorFormat("not found actor service cmd:{cmd}", cmd);

        }

        public ValueTask AsyncAction(long id, int cmd, OpenAccess access, params object[] args)
        {
            if (ActorCollect.TryGetValue(cmd, out Actor m))
                return m.AsyncAction(id, cmd, access, args);
            else
                throw new NetxException($"not found actor service cmd:{cmd}", ErrorType.ActorErr);
        }

        public ValueTask<T> CallFunc<T>(long id, int cmd, OpenAccess access, params object[] args)
        {
            if (ActorCollect.TryGetValue(cmd, out Actor m))
                return m.AsyncFunc<T>(id, cmd, access, args);
            else
                throw new NetxException($"not found actor service cmd:{cmd}", ErrorType.ActorErr);
        }



        public override async Task<IResult> AsyncFunc(int cmdTag, params object[] args)
        {
            var Id = IdsManager.MakeId;

            var result = await this.CallFunc<object>(Id, cmdTag, OpenAccess.Internal, args);

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

        public override Task<T> AsyncFunc<T>(int cmdTag, params object[] args)
        {
            return this.CallFunc<T>(IdsManager.MakeId, cmdTag, OpenAccess.Internal, args).AsTask();
        }

        public override async Task AsyncAction(int cmdTag, params object[] args)
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
}
