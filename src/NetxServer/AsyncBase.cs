using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Interface;
using Netx.Loggine;
using System;
using ZYSocket.FiberStream;

namespace Netx.Service
{
    public abstract class AsyncBase : NetxFodyInstance
    {
        public IServiceProvider Container { get; }

        public IFiberRw<AsyncToken>? FiberRw { get; protected set; }

        public long SessionId { get; }



        public AsyncBase(IServiceProvider container, IFiberRw<AsyncToken> fiberRw, long sessionId)
            : base(new DefaultLog(container.GetRequiredService<ILogger<AsyncToken>>())
                 , container.GetRequiredService<IIds>())
        {
            Container = container;
            SessionId = sessionId;
            FiberRw = fiberRw;
            IsConnect = true;
            IWrite = fiberRw;
        }





    }
}
