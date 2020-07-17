using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Interface;
using Netx.Loggine;
using System;
using System.Threading.Tasks;
using ZYSocket.Interface;

namespace Netx.Actor
{
    public abstract class ActorRunBase : INetxBuildInterface
    {

        public IServiceProvider Container { get; }
        public ActorScheduler ActorScheduler { get; }
        public IIds IdsManager { get; }
        public ILog Log { get; }

        public ActorRunBase(IServiceProvider container,ILogger logger)
        {
            Container = container;       
            Log = new DefaultLog(logger);
            IdsManager = container.GetRequiredService<IIds>();
            SerializationPacker.Serialization ??= container.GetRequiredService<ISerialization>();
            var actorscheduler = container.GetService<ActorScheduler>();
            ActorScheduler = actorscheduler ?? ActorScheduler.LineByLine;
        }

        public abstract Task<IResult> AsyncFunc(int cmdtag, params object[] args);


        public abstract Task<T> AsyncFunc<T>(int cmdtag, params object[] args);


        public abstract Task AsyncAction(int cmdTag, params object[] args);


        public abstract void Action(int cmdTag, params object[] args);


        public object Func(int cmdTag, Type type, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
