using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Interface;
using Netx.Loggine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.FiberStream;

namespace Netx.Service
{
    public abstract class AsyncBase : NetxFodyInstance
    {
        public IServiceProvider Container { get; }

        public IFiberRw<AsyncToken> FiberRw { get; protected set; }

        public long SessionId { get; }      

        public DateTime DisconnectTime { get; protected set; } = DateTime.MaxValue;

        public AsyncBase(IServiceProvider container, IFiberRw<AsyncToken> fiberRw, long sessionId)
        {
            Container = container;
            SessionId = sessionId;
            FiberRw = fiberRw;
            IsConnect = true;
            IWrite = fiberRw;
            LoggerFactory = container.GetRequiredService<ILoggerFactory>();
            Log = new DefaultLog(LoggerFactory.CreateLogger<AsyncToken>());
            IdsManager = container.GetRequiredService<IIds>();
        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual Task SendResult(long id, object argument)
        {
            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {
                    wr.WriteLen();
                    wr.Cmd(2500);
                    wr.Write(id);
                    wr.Write(false);
                    wr.Write(1);
                    wr.Write(SerializationPacker.PackSingleObject(argument));
                    return wr.Flush();
                }
            }
            else
                throw new NullReferenceException("FiberRw is null!");

        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual Task SendResult(long id, byte[][] arguments = null)
        {
            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {
                    wr.WriteLen();
                    wr.Cmd(2500);
                    wr.Write(id);

                    wr.Write(false);

                    if (arguments is null)
                        wr.Write(0);
                    else
                    {
                        wr.Write(arguments.Length);
                        foreach (var item in arguments)
                            wr.Write(item);
                    }

                    return wr.Flush();
                }
            }
            else
                throw new NullReferenceException("FiberRw is null!");

        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual Task SendResult(Result result)
        {
            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {
                    wr.WriteLen(); //为了兼容其他框架和其他的语言,还是发个长度吧
                    wr.Cmd(2500);
                    wr.Write(result.Id);

                    if (result.IsError)
                    {
                        wr.Write(true);
                        wr.Write(result.ErrorId);
                        wr.Write(result.ErrorMsg);
                    }
                    else
                    {
                        wr.Write(false);
                        wr.Write(result.Arguments.Count);
                        foreach (var item in result.Arguments)
                            wr.Write(item);
                    }

                    return wr.Flush();
                }
            }
            else
                throw new NullReferenceException("FiberRw is null!");

        }

        /// <summary>
        /// 发送错误
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <param name="errorType"></param>
        /// <returns></returns>
        protected virtual Task SendError(long id, string msg, ErrorType errorType)
        {

            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {
                    wr.WriteLen();
                    wr.Cmd(2500);
                    wr.Write(id);
                    wr.Write(true);
                    wr.Write((int)errorType);
                    wr.Write(msg);

                    return wr.Flush();
                }
            }
            else
                throw new NullReferenceException("FiberRw is null!");
        }

        /// <summary>
        /// 发送Session
        /// </summary>
        /// <returns></returns>
        public virtual Task SendSessionId()
        {
            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {
                    wr.WriteLen();
                    wr.Cmd(2000);
                    wr.Write(SessionId);
                    return wr.Flush();
                }
            }
            else
                throw new NullReferenceException("FiberRw is null!");
        }

    }
}
