using System;
using Netx.Service.Builder;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ChatServer.AsyncControllers;
using ChatServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
                  .ConfigureLogSet(p=>
                  {
                      p.AddConsole();
                      p.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error);
                  })
                 .ConfigNetWork(p =>
                 {                
                     
                     p.Port = 3000;
                 })
                 .RegisterService(Assembly.GetExecutingAssembly())
                 .RegisterDescriptors(p=>p.AddSingleton<UserManager, UserManager>())
                 .RegisterDescriptors(p=>p.AddDbContext<UserDatabaseContext>(option=>
                 {
                     option.UseSqlite("Data Source=UserDatabase.db3");

                 }))
                 .Build();

            server.Start();

            Console.ReadLine();
        }
    }
}
