using System;
using System.Buffers;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Interface;
using ZYSocket.Interface;
using ZYSocket.Share;

namespace Netx.Client
{
    public interface INetxSClientBuilder
    {
        IServiceCollection Container { get; }
        IServiceProvider? Provider { get; }

        INetxSClient Build();
        INetxSClientBuilder ConfigSessionStore(Func<ISessionStore>? func = null);
        INetxSClientBuilder ConfigConnection(Action<ConnectOption>? config = null);
        INetxSClientBuilder ConfigEncode(Func<Encoding>? func = null);
        INetxSClientBuilder ConfigIAsyncSend(Func<IAsyncSend>? func = null);
        INetxSClientBuilder ConfigIIds(Func<IServiceProvider, IIds>? func = null);
        INetxSClientBuilder ConfigISend(Func<ISend>? func = null);
        INetxSClientBuilder ConfigMemoryPool(Func<MemoryPool<byte>>? func = null);
        INetxSClientBuilder ConfigObjFormat(Func<ISerialization>? func = null);
        INetxSClientBuilder ConfigSSL(Action<SslOption>? config = null);
        INetxSClientBuilder ConfigCompress(Action<CompressOption>? config = null);
        INetxSClientBuilder ConfigureLogSet(Action<ILoggingBuilder>? config = null);
        void Dispose();
    }
}