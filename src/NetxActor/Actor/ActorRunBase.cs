using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Interface;
using Netx.Loggine;
using System;
using ZYSocket.Interface;

namespace Netx.Actor
{
    public abstract class ActorRunBase : NetxBase
    {

        public IServiceProvider Container { get; }
        public ActorScheduler ActorScheduler { get; }
     

        public ActorRunBase(IServiceProvider container)
        {
            Container = container;
            var loggerFactory = container.GetRequiredService<ILoggerFactory>();
            Log = new DefaultLog(loggerFactory.CreateLogger("Actor Run->"));
            IdsManager = container.GetRequiredService<IIds>();
            SerializationPacker.Serialization = SerializationPacker.Serialization??container.GetRequiredService<ISerialization>();
            var actorscheduler = container.GetService<ActorScheduler>();
            ActorScheduler = actorscheduler ?? ActorScheduler.LineByLine;
        }

    }
}
