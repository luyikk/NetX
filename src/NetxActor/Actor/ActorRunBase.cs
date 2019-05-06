using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Interface;
using Netx.Loggine;
using System;

namespace Netx.Actor
{
    public abstract class ActorRunBase : NetxBase
    {

        public IServiceProvider Container { get; }

        public ActorRunBase(IServiceProvider container)
        {
            Container = container;
            this.LoggerFactory = container.GetRequiredService<ILoggerFactory>();
            Log = new DefaultLog(container.GetRequiredService<ILogger<ActorRun>>());
            IdsManager = container.GetRequiredService<IIds>();
        }

    }
}
