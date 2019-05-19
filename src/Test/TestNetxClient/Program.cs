using Netx;
using Netx.Client;
using System;
using System.Threading.Tasks;

namespace TestNetxClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
          
            var client = new NetxSClientBuilder()
             .ConfigConnection(p => //配置服务器IP
             {
                 p.Host = "127.0.0.1";
                 p.Port = 1006;
                 p.VerifyKey = "123123";
             })
             //设置SESSION 的存储方式,SESSION 用来记录你的TOKEN,方便断线重连不会丢失工作进度,我们存储在内存,也可以保存成文件
            // .ConfigSessionStore(() => new Netx.Client.Session.SessionMemory())
            .ConfigSessionStore(() => new Netx.Client.Session.SessionFile())
             .Build();

            client.LoadInstance(new ClientTestController()); //加载客户端控制器供服务区主动调用,

             //client.Open(); 你可以先连接服务器,或者不连接,如果你没有OPEN 那么调用的时候


            var server = client.Get<IServer>(); //根据接口返回 服务器调用的实例

            var pcs = await server.Testnull(Guid.NewGuid(),"XCM",123);

            var cvs = await server.TestPermission();

            try
            {
                cvs = await server.TestPermission2();
            }
            catch (NetxException er)
            {
                client.Log.Error(er.Message);
            }

            for (int i = 0; i < 1000; i++)
            {
                var c = await server.Add(i, 0); //调用RPC
                client.Log.Trace(c);
            }

            for (int i = 0; i < 1000; i++)
            {
                var c = await server.AddActor(i, 0); //调用ACTOR
                client.Log.Trace(c);
            }


            for (int i = 0; i < 1000; i++)
            {
                var c = await server.RotueToAddActor(i, 0); //调用RPC TO ACTOR 路由版
                client.Log.Trace(c);
            }

            for (int i = 0; i < 1000; i++)
            {
                var c = await server.ClientAdd(i, 0); //调用RPC,这个服务器将请求路由到自己的控制器中 自己算 Add
                client.Log.Trace(c);
            }


            try
            {
                await server.ClientAddOne(1); //测试异常
            }
            catch (NetxException er)
            {
                client.Log.Error(er);
            }

            var stop = System.Diagnostics.Stopwatch.StartNew(); //测试双向递归函数
            int a = await server.RecursiveTest(1000);
            stop.Stop();
            client.Log.Info($"recursive is {a} time:{stop.ElapsedMilliseconds} ms");


            try
            {
                await server.TestTimeOut(); //超时测试

            }
            catch (NetxException er)
            {
                client.Log.Error(er.Message);
            }


            server.RunMsg("close");

            server.Finsh(); //断线测试

            Console.ReadLine();
        }
    }
}
