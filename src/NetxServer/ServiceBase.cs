using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Actor;
using Netx.Loggine;
using System;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.FiberStream;
using ZYSocket.Interface;

namespace Netx.Service
{
    public abstract class ServiceBase
    {
        /// <summary>
        /// 日记工厂类
        /// </summary>
        protected ILoggerFactory LoggerFactory { get; set; }
        /// <summary>
        /// 日记输出
        /// </summary>
        protected ILog Log { get; private set; }


        protected IServiceProvider Container { get; }



        public ServiceBase(IServiceProvider container)
        {
            LoggerFactory = container.GetRequiredService<ILoggerFactory>();
            Log = new DefaultLog(LoggerFactory.CreateLogger("NetxService"));
            SerializationPacker.Serialization = container.GetRequiredService<ISerialization>();
            Container = container;

            var actor_run = container.GetRequiredService<ActorRun>();
            foreach (var @event in container.GetServices<ActorEventBase>())            
                actor_run.EventSourcing += @event.ActorEventSourcing;            
        }


        protected Task SendToKeyError(IFiberRw fiberRw, bool iserr = false, string msg = "success")
        {
            using (var wrtokenerr = new WriteBytes(fiberRw))
            {
                wrtokenerr.WriteLen();
                wrtokenerr.Cmd(1000);
                wrtokenerr.Write(iserr);
                wrtokenerr.Write(msg);
                return wrtokenerr.Flush();
            }
        }

        protected Task SendToMessage(IFiberRw fiberRw, string msg)
        {
            using (var wrtokenerr = new WriteBytes(fiberRw))
            {
                wrtokenerr.WriteLen();
                wrtokenerr.Cmd(1001);
                wrtokenerr.Write(msg);
                return wrtokenerr.Flush();
            }
        }
    }
}
