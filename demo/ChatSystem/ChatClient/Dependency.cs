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
                    p.AddDebug().SetMinimumLevel(LogLevel.Trace);
                })
                .ConfigSSL(p =>
                {
                    p.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                        $"{System.Windows.Forms.Application.StartupPath}/client.pfx"
                        , "testPassword");
                    p.IsUse = true;
                })
               // .ConfigSessionStore(() => new Netx.Client.Session.SessionFile()) //如何需要下次打开 不用登陆
                .ConfigConnection(p =>
                {
                    p.Host = "32km.com";
                    p.Port = 3000;
                    p.ServiceName = "MessageService";
                    p.VerifyKey = "123123";
                });

            Client= icontainer.Build();
            IContainer = icontainer.Container;
        }
    }
}
