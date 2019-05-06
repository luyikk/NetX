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
                 p.Port = 1006;
             })
             .ConfigSessionStore(() => new Netx.Client.Session.SessionMemory())
             .Build();

            client.LoadInstance(new ClientTestController());

            var server = client.Get<IServer>();

            //var c = await server.AddActor(1, 2);
            //Console.WriteLine(c);
          

            for (int i = 0; i < 1000; i++)
            {
                var c = await server.Add(i, 0);
                Console.WriteLine(c);
            }
          

            Console.ReadLine();
        }
    }
}
