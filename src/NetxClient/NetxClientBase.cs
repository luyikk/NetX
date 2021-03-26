using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netx.Interface;
using Netx.Loggine;
using System;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.Interface;

namespace Netx.Client
{
    public abstract class NetxClientBase : NetxFodyInstance
    {

        /// <summary>
        /// 连接配置
        /// </summary>
        public ConnectOption ConnectOption { get; }

        /// <summary>
        /// Session管理器
        /// </summary>
        public ISessionStore Session { get; protected set; }

        /// <summary>
        /// DI 容器
        /// </summary>
        public IServiceProvider Container { get; }

        public NetxClientBase(IServiceProvider container)
            : base(new DefaultLog(container.GetRequiredService<ILogger<NetxSClient>>())
                 , container.GetRequiredService<IIds>())
        {
            Container = container;
            ConnectOption = container.GetRequiredService<IOptions<ConnectOption>>().Value;
            Session = container.GetRequiredService<ISessionStore>();
            SerializationPacker.Serialization = container.GetRequiredService<ISerialization>();
            Task.Factory.StartNew(RunRequestCheck);
        }

        protected async void RunRequestCheck()
        {
            while (true)
            {
                await Task.Delay(1000);

                if (ConnectOption.RequestTimeOut > 0)
                    RequestTimeOutHandle();
            }
        }

        /// <summary>
        /// 发送验证
        /// </summary>
        /// <returns></returns>
        protected async Task SendVerify()
        {
            if (IWrite == null)
                throw new NullReferenceException("IWrite is null!");

            Task WSend()
            {
                IWrite!.Write(1000);
                IWrite!.Write(ConnectOption.ServiceName ?? "");
                IWrite!.Write(ConnectOption.VerifyKey ?? "");
                IWrite!.Write(Session.GetSessionId());
#if NETSTANDARD2_0
                return IWrite!.Flush();
#else
                return IWrite!.FlushAsync();
#endif
            }

            await await IWrite.Sync.Ask(WSend);
        }

        /// <summary>
        /// 发送错误异步没结果模式
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <param name="errorType"></param>
        protected virtual async void AsyncSendError(long id, string msg, ErrorType errorType)
        {
            try
            {
                await SendError(id, msg, errorType);
            }
            catch(Exception er)
            {
                Log.Error("AsyncSendError:", er);
            }

        }


        protected virtual async Task GetSessionId()
        {

            if (IWrite == null)
                throw new NullReferenceException("IWrite is null!");

            Task WSend()
            {
                if (mode == 0)
                {
                    IWrite!.Write(2000);
#if NETSTANDARD2_0
                    return IWrite!.Flush();
#else
                    return IWrite!.FlushAsync();
#endif
                }
                else
                {
                    using var buffer = new WriteBytes(IWrite);
                    buffer.WriteLen();
                    buffer.Cmd(2000);
#if NETSTANDARD2_0
                    return buffer.Flush();
#else
                    return buffer.FlushAsync();
#endif
                }
            }

            await await IWrite.Sync.Ask(WSend);

        }

        /// <summary>
        /// 发送错误
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <param name="errorType"></param>
        /// <returns></returns>
        protected virtual async Task SendError(long id, string msg, ErrorType errorType)
        {

            if (IWrite == null)
                throw new NullReferenceException("IWrite is null!");

            Task WSend()
            {
                if (mode == 0)
                {
                    IWrite!.Write(2500);
                    IWrite!.Write(id);
                    IWrite!.Write(true);
                    IWrite!.Write((int)errorType);
                    IWrite!.Write(msg);
#if NETSTANDARD2_0
                    return IWrite!.Flush();
#else
                    return IWrite!.FlushAsync();
#endif
                }
                else
                {
                    using var buffer = new WriteBytes(IWrite);
                    buffer.WriteLen();
                    buffer.Cmd(2500);
                    buffer.Write(id);
                    buffer.Write(true);
                    buffer.Write((int)errorType);
                    buffer.Write(msg);
#if NETSTANDARD2_0
                    return buffer.Flush();
#else
                    return buffer.FlushAsync();
#endif



                }
            }

            await await IWrite.Sync.Ask(WSend);
        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual async Task SendResult(long id, object argument)
        {
            if (IWrite == null)
                throw new NullReferenceException("IWrite is null!");

            var packbuffer = SerializationPacker.PackSingleObject(argument);

            Task WSend()
            {
                if (mode == 0)
                {
                    IWrite!.Write(2500);
                    IWrite!.Write(id);
                    IWrite!.Write(false);
                    IWrite!.Write(1);
                    IWrite!.Write(packbuffer);
#if NETSTANDARD2_0
                    return IWrite!.Flush();
#else
                    return IWrite!.FlushAsync();
#endif
                }
                else
                {
                    using var buffer = new WriteBytes(IWrite);
                    buffer.WriteLen();
                    buffer.Cmd(2500);
                    buffer.Write(id);
                    buffer.Write(false);
                    buffer.Write(1);
                    buffer.Write(packbuffer);
#if NETSTANDARD2_0
                    return buffer.Flush();
#else
                    return buffer.FlushAsync();
#endif
                }
            }
            await await IWrite.Sync.Ask(WSend);
        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual async Task SendResult(long id, byte[][]? arguments = null)
        {
            if (IWrite == null)
                throw new NullReferenceException("IWrite is null!");

            Task WSend()
            {
                if (mode == 0)
                {

                    IWrite!.Write(2500);
                    IWrite!.Write(id);
                    IWrite!.Write(false);
                    if (arguments is null)
                        IWrite!.Write(0);
                    else
                    {
                        IWrite!.Write(arguments.Length);
                        foreach (var item in arguments)
                            IWrite!.Write(item);
                    }

#if NETSTANDARD2_0
                    return IWrite!.Flush();
#else
                    return IWrite!.FlushAsync();
#endif
                }
                else
                {
                    using var buffer = new WriteBytes(IWrite);
                    buffer.WriteLen();
                    buffer.Cmd(2500);
                    buffer.Write(id);
                    buffer.Write(false);
                    if (arguments is null)
                        buffer.Write(0);
                    else
                    {
                        buffer.Write(arguments.Length);
                        foreach (var item in arguments)
                            buffer.Write(item);
                    }

#if NETSTANDARD2_0
                    return buffer.Flush();
#else
                    return buffer.FlushAsync();
#endif
                }
            }

            await await IWrite.Sync.Ask(WSend);
        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual async Task SendResult(Result result)
        {
            if (IWrite == null)
                throw new NullReferenceException("IWrite is null!");

            Task WSend()
            {
                if (mode == 0)
                {
                    IWrite!.Write(2500);
                    IWrite!.Write(result.Id);

                    if (result.IsError)
                    {
                        IWrite!.Write(true);
                        IWrite!.Write(result.ErrorId);
                        IWrite!.Write(result.ErrorMsg ?? "");
                    }
                    else
                    {
                        IWrite!.Write(false);
                        IWrite!.Write(result.Arguments?.Count ?? 0);
                        if (result.Arguments != null)
                            foreach (var item in result.Arguments)
                                IWrite!.Write(item);
                    }

#if NETSTANDARD2_0
                    return IWrite!.Flush();
#else
                    return IWrite!.FlushAsync();
#endif
                }
                else
                {
                    using var buffer = new WriteBytes(IWrite);
                    buffer.WriteLen();
                    buffer.Cmd(2500);
                    if (result.IsError)
                    {
                        buffer.Write(true);
                        buffer.Write(result.ErrorId);
                        buffer.Write(result.ErrorMsg ?? "");
                    }
                    else
                    {
                        buffer.Write(false);
                        buffer.Write(result.Arguments?.Count ?? 0);
                        if (result.Arguments != null)
                            foreach (var item in result.Arguments)
                                buffer.Write(item);
                    }

#if NETSTANDARD2_0
                    return buffer.Flush();
#else
                    return buffer.FlushAsync();
#endif
                }
            }

            await await IWrite.Sync.Ask(WSend);

        }
    }
}
