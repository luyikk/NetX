using Netx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netx.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography.X509Certificates;

namespace ChatClient
{
    public static class Dependency
    {
        static X509Certificate certificate = new X509Certificate2(Environment.CurrentDirectory + "/client.pfx", "testPassword");
        public static INetxSClient Client { get; }
        public static IServiceCollection IContainer { get; }

        static Dependency()
        {
            var icontainer = new NetxSClientBuilder()
                .ConfigureLogSet(p =>
                {
                    p.AddDebug().SetMinimumLevel(LogLevel.Trace); //添加DEBUG日记输出
                })
                .ConfigSSL(p => //设置SSL加密
                {
                    p.Certificate = certificate;
                    p.IsUse = true;
                })
                 .ConfigCompress(p => p.Mode = Netx.CompressType.None)
                // .ConfigSessionStore(() => new Netx.Client.Session.SessionFile()) //如何保存Session需要下次打开 不用登陆
                .ConfigConnection(p => //配置连接
                {
                    p.Host = "127.0.0.1"; //IP
                    p.Port = 3000;  //端口
                    p.ServiceName = "MessageService"; //服务名称
                    p.VerifyKey = "123123"; //密码
                });

            Client= icontainer.Build(); //生成一个CLIENT
            IContainer = icontainer.Container;
            
        }
    }
}
