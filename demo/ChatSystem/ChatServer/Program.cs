using System;
using Netx.Service.Builder;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ChatServer.AsyncControllers;

namespace ChatServer
{
    class Program
    {
        static void Main()
        {
           
            var server = new NetxServBuilder()
                 .ConfigBase(p =>
                 {
                     p.ServiceName = "MessageService";
                     p.VerifyKey = "123123";
                     p.ClearSessionTime = 6000;
                 })
                  .ConfigSSL(p =>
                  {
                      p.Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                          $"{new System.IO.DirectoryInfo(Assembly.GetExecutingAssembly().Location).Parent}/server.pfx",
                          "testPassword");
                      p.IsUse = true;
                  })
                 
                 .ConfigNetWork(p =>
                 {
                     p.Port = 3000;
                 })
                 .RegisterService(Assembly.GetExecutingAssembly())
                 .RegisterDescriptors(p=>p.AddSingleton<UserManager, UserManager>())
                 .Build();

            server.Start();

            Console.ReadLine();
        }
    }
}
