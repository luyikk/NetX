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
            Log = new DefaultLog(container.GetRequiredService<ILogger<NetxSClient>>());
            ConnectOption = container.GetRequiredService<IOptions<ConnectOption>>().Value;
            Session = container.GetRequiredService<ISessionStore>();
            SerializationPacker.Serialization = container.GetRequiredService<ISerialization>();
            IdsManager = container.GetRequiredService<IIds>();
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
    }
}
