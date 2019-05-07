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
            Log = new DefaultLog(LoggerFactory.CreateLogger("Actor Run->"));
            IdsManager = container.GetRequiredService<IIds>();
        }

    }
}
