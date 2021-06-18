using Netx.Client;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace TestRustServer
{
    class Program
    {
        static X509Certificate certificate = new X509Certificate2(Environment.CurrentDirectory + "/cert.pfx", "testPassword");

        static async Task Main(string[] args)
        {
            var client = new NetxSClientBuilder()
             .ConfigConnection(p => //配置服务器IP
             {
                 p.Host = "127.0.0.1";
                 p.Port = 6666;
                 p.VerifyKey = "123123";
                 p.MaxPackerSize = 256 * 1024;
             })
              .ConfigSSL(p =>
              {
                  p.IsUse = false;
                  p.Certificate = null;
              })
            //设置SESSION 的存储方式,SESSION 用来记录你的TOKEN,方便断线重连不会丢失工作进度,我们存储在内存,也可以保存成文件
            // .ConfigSessionStore(() => new Netx.Client.Session.SessionMemory())
            .ConfigSessionStore(() => new Netx.Client.Session.SessionFile())
             .Build();

            client.LoadInstance(new ClientTestController()); //加载客户端控制器供服务区主动调用,

            client.Open(); //你可以先连接服务器,或者不连接,如果你没有OPEN 那么调用的时候

            var server = client.Get<IServer>(); //根据接口返回 服务器调用的实例
            await server.Print(5);
            await server.RunTest("joy");
            var x = await server.ToClientAddOne(1);
            Console.WriteLine("x:{0}",x);
            await server.Print2(6, "my name is");

            var stop = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 100000; i++)            
                await server.Add(1, i);
            
            var r=  await  server.RecursiveTest(10000);
            Console.WriteLine($"{r} {stop.ElapsedMilliseconds}");

            var res = new LogOn
            {
                Username = "username",
                Password = "password"
            };

            var (success, msg) = await server.LogOn(res);
            Console.WriteLine($"{success} {msg}");

            var res2 = await server.LogOn2(("username", "password"));
            Console.WriteLine($"{res2.Success} {res2.Msg}");

            Console.ReadLine();
        }
    }
}
