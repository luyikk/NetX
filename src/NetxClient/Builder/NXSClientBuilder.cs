﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netx.Interface;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using ZYSocket.Client;
using ZYSocket.FiberStream;
using ZYSocket.Interface;
using ZYSocket.Share;

namespace Netx.Client
{
    public class NetxSClientBuilder : INetxSClientBuilder
    {
        public IServiceCollection Container { get; }
        public IServiceProvider? Provider { get; private set; }

        public NetxSClientBuilder()
        {
            Container = new ServiceCollection();
            Container.AddOptions();
            Container.AddTransient<SocketClient, SocketClient>(p =>
            {
                var config = p.GetRequiredService<IOptions<ConnectOption>>().Value;

                return new SocketClient(buffer_size: config.BufferSize,
                    maxPackerSize: config.MaxPackerSize,
                    memPool: p.GetRequiredService<MemoryPool<byte>>(),
                    sync_send: p.GetRequiredService<ISend>(),
                    async_send: p.GetRequiredService<IAsyncSend>(),
                    obj_Format: p.GetRequiredService<ISerialization>(),
                    encode: p.GetRequiredService<Encoding>());
            });
            ConfigureDefaults();
        }

        private void ConfigureDefaults()
        {
            ConfigureLogSet();
            ConfigEncode();
            ConfigSessionStore();
            ConfigMemoryPool();
            ConfigISend();
            ConfigIAsyncSend();
            ConfigObjFormat();
            ConfigIIds();
        }

        public INetxSClientBuilder ConfigConnection(Action<ConnectOption>? config = null)
        {
            if (config != null)
                Container.Configure(config);

            return this;
        }


        public INetxSClientBuilder ConfigEncode(Func<Encoding>? func = null)
        {
            Container.AddSingleton(p =>
            {
                if (func is null)
                    return Encoding.UTF8;
                else
                    return func();
            });

            return this;
        }

        public INetxSClientBuilder ConfigMemoryPool(Func<MemoryPool<byte>>? func = null)
        {
            Container.AddTransient(p =>
            {
                if (func is null)
                {
                    var config = p.GetRequiredService<IOptions<ConnectOption>>().Value;
                    return new Thruster.FastMemoryPool<byte>(config.MaxPackerSize);
                }
                else
                    return func();
            });

            return this;
        }


        public INetxSClientBuilder ConfigISend(Func<ISend>? func = null)
        {
            Container.AddTransient(p =>
            {
                if (func is null)
                    return new PoolSend(true);
                else
                    return func();
            });

            return this;
        }


        public INetxSClientBuilder ConfigIAsyncSend(Func<IAsyncSend>? func = null)
        {
            Container.AddTransient(p =>
            {
                if (func is null)
                    return new PoolSend(true);
                else
                    return func();
            });

            return this;
        }

        public INetxSClientBuilder ConfigSessionStore(Func<ISessionStore>? func = null)
        {

            Container.AddTransient(p =>
            {
                if (func is null)
                    return new Session.SessionMemory();
                else
                    return func();
            });
            return this;
        }

        public INetxSClientBuilder ConfigObjFormat(Func<ISerialization>? func = null)
        {
            Container.AddTransient(p =>
            {
                if (func is null)
                    return new MsgpkgFormat();
                else
                    return func();
            });

            return this;
        }


        public INetxSClientBuilder ConfigIIds(Func<IServiceProvider, IIds>? func = null)
        {
            if (func is null)
                Container.AddScoped<IIds, DefaultMakeIds>();
            else
                Container.AddScoped(func);

            return this;
        }

        public INetxSClientBuilder ConfigSSL(Action<SslOption>? config = null)
        {
            if (config != null)
                Container.Configure(config);
            return this;
        }

        public INetxSClientBuilder ConfigCompress(Action<CompressOption>? config = null)
        {
            if (config != null)
                Container.Configure(config);
            return this;
        }

        public INetxSClientBuilder ConfigureLogSet(Action<ILoggingBuilder>? config = null)
        {
            if (config is null)
            {
                Container.AddLogging(p =>
                {
                    p.AddConsole();
                    p.SetMinimumLevel(LogLevel.Trace);
                });
            }
            else
            {
                Container.AddLogging(config);
            }

            return this;
        }

        public INetxSClient Build()
        {
            if (Provider is null)
            {
                Container.TryAdd(ServiceDescriptor.Scoped(p => new NetxSClient(p)));
                Provider = Container.BuildServiceProvider();
                return Provider.GetRequiredService<NetxSClient>();
            }
            else
                return Provider.GetRequiredService<NetxSClient>();
        }



        public void Dispose()
        {
            if (Provider is IDisposable disposable)
                disposable.Dispose();
        }

    }
}
