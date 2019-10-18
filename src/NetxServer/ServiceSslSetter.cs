using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.FiberStream;

namespace Netx.Service
{
    public abstract class ServiceSslSetter : ServiceToken
    {

        protected readonly bool is_use_ssl;
        protected X509Certificate? Certificate { get; }

        public ServiceSslSetter(IServiceProvider container) : base(container)
        {
            var ssloption = container.GetRequiredService<IOptions<SslOption>>().Value;
            if (ssloption.IsUse && ssloption.Certificate != null)
            {
                Certificate = ssloption.Certificate;
                is_use_ssl = true;
            }
        }

        /// <summary>
        /// 返回FiberRw
        /// </summary>
        /// <param name="socketAsync"></param>
        /// <returns></returns>
        protected virtual async Task<IFiberRw<AsyncToken>?> GetFiberRw(ISockAsyncEventAsServer socketAsync)
        {
            if (is_use_ssl) //SSL Config
            {
                var (fiber, msg) = await socketAsync.GetFiberRwSSL<AsyncToken>(Certificate!);

                if (fiber is null)
                {
                    if(msg!=null)
                        Log.Error(msg);                  
                    return null;
                }
                else
                    return fiber;
            }
            else
                return await socketAsync.GetFiberRw<AsyncToken>();
        }
    }
}
