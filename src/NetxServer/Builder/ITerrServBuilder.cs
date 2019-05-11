using System;
using System.Buffers;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Actor;
using Netx.Interface;
using ZYSocket.Interface;
using ZYSocket.Server.Builder;
using ZYSocket.Share;

namespace Netx.Service.Builder
{
    public interface INetxServBuilder : IDisposable
    {
        IServiceCollection Container { get; }
        IServiceProvider Provider { get; }
        SockServBuilder SockServConfig { get; }

        NetxService Build();
        INetxServBuilder ConfigEncode(Func<Encoding> func = null);
        INetxServBuilder ConfigIAsyncSend(Func<IAsyncSend> func = null);
        INetxServBuilder ConfigISend(Func<ISend> func = null);
        INetxServBuilder ConfigMemoryPool(Func<MemoryPool<byte>> func = null);
        INetxServBuilder ConfigObjFormat(Func<ISerialization> func = null);
        INetxServBuilder ConfigSocketServer(Action<SocketServerOptions> config = null);
        INetxServBuilder ConfigureActorScheduler(Func<IServiceProvider, ActorScheduler> func = null);
        INetxServBuilder ConfigureDefaults();
        INetxServBuilder ConfigureKey(Action<OptionKey> config = null);
        INetxServBuilder ConfigureLogSet(Action<ILoggingBuilder> config = null);
        INetxServBuilder ConfigIIds(Func<IServiceProvider, IIds> func = null);
        INetxServBuilder ConfigSSL(Action<SslOption> config = null);
        INetxServBuilder ConfigSession(Action<SessionOption> config = null);
        INetxServBuilder AddActorEvent<T>() where T : ActorEventBase;
        INetxServBuilder RegisterService(Assembly assembly);
        INetxServBuilder RegisterService(Type controller_instance_type);
        INetxServBuilder RegisterDescriptors(Action<IServiceCollection> serviceDescriptors);
    }
}