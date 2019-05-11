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

        public event EventHandler<ActorMessage> EventSourcing;

        public ActorRun(IServiceProvider container)
            : base(container)
        {
            actorCollect = new Lazy<ConcurrentDictionary<int, Actor<R>>>(true);
            Load();
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

        private void Actor_CompletedEvent(object sender, ActorMessage e)
        {
            EventSourcing?.Invoke(sender, e);           
        }

        public MethodRegister GetCmdService(int cmd)
        {
            if (ActorCollect.ContainsKey(cmd))
            {
                return ActorCollect[cmd].CmdDict[cmd];
            }
            else
                return null;

        }

        public void CallAction(long id, int cmd, params object[] args)
        {
            if (ActorCollect.ContainsKey(cmd))
                ActorCollect[cmd].Action(id, cmd, args);
            else
                Log.Error($"not find actor service cmd:{cmd}");

        }

        public async Task CallAsyncAction(long id, int cmd, params object[] args)
        {
            if (ActorCollect.ContainsKey(cmd))
                await ActorCollect[cmd].AsyncAction(id, cmd, args);
            else
                throw new NetxException($"not find actor service cmd:{cmd}", ErrorType.ActorErr);
        }

        public async Task<R> CallAsyncFunc(long id, int cmd, params object[] args)
        {
            if (ActorCollect.ContainsKey(cmd))
                return await ActorCollect[cmd].AsyncFunc(id, cmd, args);
            else
                throw new NetxException($"not find actor service cmd:{cmd}", ErrorType.ActorErr);
        }

        protected override Task SendAsyncAction(int cmdTag, long Id, object[] args)
        {
            return CallAsyncAction(Id, cmdTag, args);
        }

        protected async override Task<IResult> AsyncFuncSend(int cmdTag, long Id, object[] args)
        {
            var result = await this.CallAsyncFunc(Id, cmdTag, args);

            switch (result)
            {
                case Result rs: return rs;
                default:
                    {
                        var rs = new Result(result)
                        {
                            Id = Id
                        };
                        return rs;
                    }
            }
        }


        public IResult GetErrorResult(string msg, long id)
        {
            Result err = new Result()
            {
                ErrorMsg = msg,
                Id = id,
                ErrorId = (int)ErrorType.ActorErr
            };

            return err;
        }


        protected override void SendAction(int cmdTag, object[] args)
        {
            this.CallAction(-1, cmdTag, args);
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
