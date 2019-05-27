using Netx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netx.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChatClient
{
    public static class Dependency
    {
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
                    p.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                        $"{System.Windows.Forms.Application.StartupPath}/client.pfx"
                        , "testPassword");
                    p.IsUse = true;
                })
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
