using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Netx;
using Netx.Client;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;

namespace TestClient
{
    class Program
    {
        static X509Certificate certificate = new X509Certificate2(Environment.CurrentDirectory + "/client.pfx", "testPassword");

        static async Task Main(string[] args)
        {

            int clientCout = Environment.ProcessorCount*8;


            var clientBuilder = new NetxSClientBuilder()
                .ConfigConnection(p =>
                {
                    p.Host = args.Length==0? "127.0.0.1":args[0];
                    p.Port = args.Length == 0 ? 1005:args.Length>=2?int.Parse(args[1]): 1005;
                    p.VerifyKey = args.Length == 0 ? "123123":args.Length>=3?args[2]:"123123";
                    p.RequestTimeOut = 0;                  
                })
                //.ConfigSSL(p =>
                //{
                //    p.IsUse = true;
                //    p.Certificate = certificate;
                //})
               
                .ConfigSessionStore(() =>
                {
                    return new Netx.Client.Session.SessionMemory();
                });


            var client0 = clientBuilder.Build();

            var server= client0.Get<IServer>();

            List<Task> RunList = new List<Task>();

            int cc = 100000;

            int threadcount = 50;

            var x = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < threadcount; i++)
            {
                async Task Runs()
                {
                    int xcount = i + cc;
                    while (i < xcount)
                    {
                        int c = i + 1;
                        i = await server.AddOne(i);
                        if (c != i)
                            throw new Exception("error value");
                    }
                }

                RunList.Add(Runs());
            }
            

            await Task.WhenAny(RunList);

            x.Stop();

            var time = x.ElapsedMilliseconds;
            double all = cc * threadcount;
            Console.WriteLine(time+" ms");
            Console.WriteLine((all/time*1000.0)+" TPS");
            Console.ReadLine();


            List<Task<(long m, int count)>> tasks = new List<Task<(long m, int count)>>(clientCout);

            tasks.Add(Run(client0));

            for (int i=0; i<clientCout-1;i++)
            {
                tasks.Add(Run(clientBuilder.Provider.CreateScope().ServiceProvider.GetRequiredService<NetxSClient>()));
            }

            double allm = 0.0;
            double count = 0.0;
         
            
            foreach (var item in tasks)
            {
                var r = await item;
                allm += r.m;
                count += r.count;
            }
            Console.WriteLine(count + "/" + (allm / clientCout));

            float mc = (float)(count / (allm / clientCout));
            float sc = mc * 1000;

            Console.WriteLine(sc + " TPS");

            var icount=  await  client0.Get<IServer>().GetAllCount();
            Console.WriteLine($"i is {icount}");
            Console.ReadLine();
        }

        static async Task<(long m,int count)> Run(NetxSClient client)
        {
            
            var server = client.Get<IServer>();

            int count = 10000;

            var x = System.Diagnostics.Stopwatch.StartNew();
            int i = new Random().Next(-10000, 10000);
            int xcount = i + count;
            while (i < xcount)
            {
                int c = i+1;
                i = await server.AddOneActor(i);              
                if (c != i)
                    throw new Exception("error value");
            }

            x.Stop();
            Console.WriteLine(x.ElapsedMilliseconds);       

            server.PrintMsg("完成");

            return (x.ElapsedMilliseconds, count);
        }
    }
}
