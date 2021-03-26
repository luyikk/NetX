using System;
using Netx.Service.Builder;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ChatServer.AsyncControllers;
using ChatServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace ChatServer
{
    class Program
    {
        static X509Certificate certificate = new X509Certificate2(Environment.CurrentDirectory + "/server.pfx", "testPassword");

        static void Main()
        {
          
           
           using var server = new NetxServBuilder()
                 .ConfigBase(p =>
                 {
                     p.ServiceName = "MessageService"; //服务名
                     p.VerifyKey = "123123";  //密码
                     p.ClearSessionTime = 60000; //Session清理时间
                 })
                  .ConfigSSL(p =>  //配置SSL加密
                  {
                      p.Certificate = certificate;
                      p.IsUse = true;
                  })
                  .ConfigCompress(p => p.Mode = Netx.CompressType.None)
                  .ConfigureLogSet(p=> //设置日记
                  {
                      p.AddConsole();
                      p.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Error); //过滤EF日记
                  })
                 .ConfigNetWork(p =>
                 {
                    // p.Host = "any";  //监听所有IP
                     p.Port = 3000; //服务端口
                 })
                 .RegisterService(Assembly.GetExecutingAssembly()) //加载当前DLL里面的所有控制器
                 .RegisterDescriptors(p=>p.AddSingleton<UserManager, UserManager>()) //添加用户管理器
                 .RegisterDescriptors(p=>p.AddDbContext<UserDatabaseContext>(option=> //设置SQL
                 {
                     option.UseSqlite("Data Source=UserDatabase.db3");

                 }))
                 .Build();

            server.Start(); //启动服务

            Console.ReadLine();
        }
    }
}
