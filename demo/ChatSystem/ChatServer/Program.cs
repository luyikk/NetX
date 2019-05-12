using System;
using Netx.Service.Builder;
using System.Reflection;
namespace ChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
           
            var server = new NetxServBuilder()
                 .ConfigBase(p =>
                 {
                     p.ServiceName = "MessageService";
                     p.VerifyKey = "123123";
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
                 .Build();

            server.Start();

            Console.ReadLine();
        }
    }
}
