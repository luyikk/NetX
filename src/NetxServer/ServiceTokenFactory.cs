using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Loggine;
using System;
using System.Collections.Concurrent;
using ZYSocket.FiberStream;

namespace Netx.Service
{
    public class ServiceTokenFactory
    {
        /// <summary>
        /// 日记输出
        /// </summary>
        protected ILog Log { get; private set; }
        /// <summary>
        /// 随机数
        /// </summary>
        protected Random Rand { get; }

        protected IServiceProvider Container { get; }

        internal ServiceTokenFactory(IServiceProvider container)
        {
            Container = container;
            Rand = new Random();
            Log = new DefaultLog(container.GetRequiredService<ILogger<ServiceTokenFactory>>());
        }

        internal AsyncToken CreateAsynToken(IFiberRw<AsyncToken> fiberRw, ConcurrentDictionary<int, MethodRegister> asyncServicesRegisterDict)
        {
            var sessionId = MakeSessionId();
            var token = new AsyncToken(Container, fiberRw, asyncServicesRegisterDict, sessionId);
            Log.TraceFormat("make token sessionId:{0}", sessionId);
            return token;
        }

        private long MakeSessionId()
        {
            lock (Rand)
            {
                long c = 630822816000000000; //2000-1-1 0:0:0:0
                long x = DateTime.Now.Ticks;
                long m = ((x - c) * 1000) + Rand.Next(1000);
                return m;
            }
        }


    }
}
