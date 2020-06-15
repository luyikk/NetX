using Netx.Client;
using NetxTestServer;
using System;
using System.Threading.Tasks;

namespace TestClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new NetxSClientBuilder()
               .ConfigConnection(p =>
               {
                   p.Host = "127.0.0.1";
                   p.Port = 50054;
                   p.RequestTimeOut = 0;
               })
               .ConfigObjFormat(() => new NetxSerializes.JSONSerializes())
               .Build();

            var a = await client.Get<ITestServer>().SayHello(new HelloRequest { Name = "123123" });

            Console.WriteLine(a.Message);
            Console.ReadLine();

        }
    }
}
