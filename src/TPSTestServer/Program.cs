using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
namespace TestServer
{
    class Program
    {
        static X509Certificate certificate = new X509Certificate2(Environment.CurrentDirectory + "/server.pfx", "testPassword");

        static void Main()
        {
            var service = new Netx.Service.Builder.NetxServBuilder()
                .RegisterService(Assembly.GetExecutingAssembly())
                .ConfigureKey(p => p.Key = "123123")
                //.ConfigSSL(p =>
                // {
                //     p.IsUse = true;
                //     p.Certificate = certificate;
                // })              
                .ConfigSocketServer(p =>
                {
                    p.MaxConnectCout = 100;
                    p.Port = 1005;

                })
                .RegisterDescriptors(p => p.AddSingleton<List<string>>(_ => new List<string>() { "1", "2", "3" }))            
                .Build();

            service.Start();

            Console.ReadLine();
        }
    }
}
