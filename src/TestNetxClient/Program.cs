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
             .ConfigConnection(p =>
             {
                 p.Host = "127.0.0.1";
                 p.Port = 1006;
             })
             .ConfigSessionStore(() => new Netx.Client.Session.SessionMemory())
             .Build();

            client.LoadInstance(new ClientTestController());

            var server = client.Get<IServer>();

             server.RunMsg();
            //Console.WriteLine(c);
          

            for (int i = 0; i < 1000; i++)
            {
                var c = await server.AddActor(i, 0);
                Console.WriteLine(c);
            }
          

            Console.ReadLine();
        }
    }
}
