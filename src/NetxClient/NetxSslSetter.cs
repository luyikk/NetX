using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.FiberStream;

namespace Netx.Client
{
    public abstract class NetxSslSetter : NetxAnalysis
    {
        protected readonly bool is_use_ssl;
        protected X509Certificate? Certificate { get; }

        public NetxSslSetter(IServiceProvider container)
        : base(container)
        {
            var ssloption = container.GetRequiredService<IOptions<SslOption>>().Value;
            if (ssloption.IsUse)
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
        protected virtual async Task<IFiberRw?> GetFiberRw(ISockAsyncEventAsClient socketAsync)
        {
            if (is_use_ssl) //SSL Config
            {
                var result =await socketAsync.GetFiberRwSSL(Certificate!);

                if (result.IsError)
                {
                    if (result.ErrMsg != null)
                        Log.Error(result.ErrMsg);
                    return null;
                }
                else
                    return result.FiberRw;
            }
            else
                return await socketAsync.GetFiberRw();
        }

    }
}
