using Interfaces;
using Netx.Client;
using System;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main()
        {
            var client = new NetxSClientBuilder()
                .ConfigConnection(p => //配置服务器IP
                {
                    p.Host = "127.0.0.1";
                    p.Port = 3000;
                    p.VerifyKey = "123123";
                })
                .ConfigSessionStore(() => new Netx.Client.Session.SessionFile())
                .Build();

            client.LoadInstance(new ClientController());

            var server = client.Get<IServer>();

            var r = await server.Add(1, 2);
            r = await server.AddOne(r);
            Console.WriteLine(r);

            var servernew = client.Get<IServerNew>();
            r = await servernew.AddTow(r);
            Console.WriteLine(r);
            servernew.Run(r.ToString());


            var serverold = client.Get<IServerOld>();
            r = await serverold.AddOne(r);
            Console.WriteLine(r);

            var serverdef = client.Get<IServerDef>();
            r = await serverdef.Add3(r);
            Console.WriteLine(r);


            Console.WriteLine("---------next actor------------");


            var actor = client.Get<IActors>();

            r = await actor.Add(1, 2);
            r = await actor.AddOne(r);
            Console.WriteLine(r);

            var actornew = client.Get<IActorsNew>();
            r = await actornew.AddTow(r);
            Console.WriteLine(r);
            actornew.Run(r.ToString());


            var actorold = client.Get<IActorsOld>();
            r = await actorold.AddOne(r);
            Console.WriteLine(r);

            var actordef = client.Get<IActorsDef>();
            r = await actordef.Add3(r);
            Console.WriteLine(r);

            try
            {
                await client.Get<IActorsSub>().Sub(5, 4);

            }
            catch (Netx.NetxException er)
            {
                Console.WriteLine(er.Message);
            }


            Console.ReadLine();
        }
    }
}
