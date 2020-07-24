using Netx.Client;
using STS;
using System;
using System.Threading.Tasks;

namespace Client
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
                 p.MaxPackerSize = 256 * 1024;
             })
            //设置SESSION 的存储方式,SESSION 用来记录你的TOKEN,方便断线重连不会丢失工作进度,我们存储在内存,也可以保存成文件
            // .ConfigSessionStore(() => new Netx.Client.Session.SessionMemory())
            // .ConfigSessionStore(() => new Netx.Client.Session.SessionFile())
             .Build();

            client.LoadInstance(new ClientController()); //加载客户端控制器供服务区主动调用,

            client.Open(); //你可以先连接服务器,或者不连接,如果你没有OPEN 那么调用的时候



            var server = client.Get<IServer>(); //根据接口返回 服务器调用的实例

            var res = await server.Login("test", "123123");


            Console.WriteLine(res);


        }
    }
}
