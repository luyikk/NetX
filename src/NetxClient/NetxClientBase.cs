using Netx.Loggine;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Netx.Interface;
using ZYSocket.Interface;
using ZYSocket.FiberStream;
using ZYSocket;

namespace Netx.Client
{
    public abstract class  NetxClientBase: NetxFodyInstance
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
        {
            Container = container;
            LoggerFactory = container.GetRequiredService<ILoggerFactory>();
            Log = new DefaultLog(container.GetRequiredService<ILogger<NetxSClient>>());
            ConnectOption = container.GetRequiredService<IOptions<ConnectOption>>().Value;
            Session = container.GetRequiredService<ISessionStore>();
            SerializationPacker.Serialization = container.GetRequiredService<ISerialization>();
            IdsManager = container.GetRequiredService<IIds>();

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
        protected Task<int> SendVerify()
        {
            IWrite.Write(1000);
            IWrite.Write(ConnectOption.VerifyKey??"");
            IWrite.Write(Session.GetSessionId());
            return IWrite.Flush();
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

            if (IWrite != null)
            {

                IWrite.Write(2500);
                IWrite.Write(id);
                IWrite.Write(true);
                IWrite.Write((int)errorType);
                IWrite.Write(msg);
                return IWrite.Flush();
                
            }
            else
                throw new NullReferenceException("IWrite is null!");
        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual Task SendResult(long id, object argument)
        {
            if (IWrite != null)
            {

                IWrite.Write(2500);
                IWrite.Write(id);
                IWrite.Write(false);
                IWrite.Write(1);
                IWrite.Write(SerializationPacker.PackSingleObject(argument));
                return IWrite.Flush();

            }
            else
                throw new NullReferenceException("IWrite is null!");

        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual Task SendResult(long id, byte[][] arguments = null)
        {
            if (IWrite != null)
            {
                IWrite.Write(2500);
                IWrite.Write(id);

                IWrite.Write(false);

                if (arguments is null)
                    IWrite.Write(0);
                else
                {
                    IWrite.Write(arguments.Length);
                    foreach (var item in arguments)
                        IWrite.Write(item);
                }

                return IWrite.Flush();

            }
            else
                throw new NullReferenceException("IWrite is null!");

        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual Task SendResult(Result result)
        {
            if (IWrite != null)
            {

                IWrite.Write(2500);
                IWrite.Write(result.Id);

                if (result.IsError)
                {
                    IWrite.Write(true);
                    IWrite.Write(result.ErrorId);
                    IWrite.Write(result.ErrorMsg);
                }
                else
                {
                    IWrite.Write(false);
                    IWrite.Write(result.Arguments.Count);
                    foreach (var item in result.Arguments)
                        IWrite.Write(item);
                }

                return IWrite.Flush();

            }
            else
                throw new NullReferenceException("IWrite is null!");

        }
    }
}
