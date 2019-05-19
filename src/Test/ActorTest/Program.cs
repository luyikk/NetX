using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx;
using Netx.Actor;
using Netx.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZYSocket.Interface;
using ZYSQL;

namespace ActorTest
{
    class Program
    {
       static  IServiceCollection Container = new ServiceCollection();
        static async Task Main(string[] args)
        {

            Container.AddSingleton<IIds, DefaultMakeIds>();
            Container.AddSingleton<ActorController, TestActorController>();
            Container.AddSingleton<ActorController, NextActorController>();
            Container.AddSingleton<ActorRun>(p => new ActorRun(p));
            Container.AddSingleton<ISerialization>(p => new ZYSocket.FiberStream.ProtobuffObjFormat());
            Container.AddLogging(p =>
            {
                p.AddConsole();
                p.SetMinimumLevel(LogLevel.Trace);
            });

            var build = Container.BuildServiceProvider();

            var Actor = build.GetRequiredService<ActorRun>();



            var server = Actor.Get<ICallServer>();
            await server.Add(0, 0);



            await server.SetUserCoin(1, 100);
            var user = await server.GetUser(1);
            Console.WriteLine($"{user.Name} current coin:{user.Coin}");


            var task1 = Task.Run(() =>
              {
                  for (int i = 0; i < 100; i++)
                  {
                      server.AddUserCoin(1, 100);
                  }

              });

            var task2 = Task.Run(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    server.SubUserCoin(1, 100);
                }

            });

            await Task.WhenAll(task1, task2);

            user = await server.GetUser(1);
            Console.WriteLine($"{user.Name} current coin:{user.Coin}");

            Console.ReadLine();
            await server.SetUserCoin(1, 100);



            #region TestCount
            var stop = System.Diagnostics.Stopwatch.StartNew();

            var x = 0;
          
            long count = 0;            

            for (int i = 0; i < 2000000; i++)
            {
                x = await server.Add(i, x);
                count++;
            }

            stop.Stop();

            var t = await server.GetV();
            Console.WriteLine(x);
            Console.WriteLine(t);
            Console.WriteLine($"Count:{count} time {stop.ElapsedMilliseconds}");


            stop.Restart();

            x = 0;
            count = 0;

            for (int i = 0; i < 2000000; i++)
            {
                x = await Actor.CallFunc(i, 2000, OpenAccess.Internal, i, x);
                count++;
            }

            stop.Stop();

            t = await server.GetV();
            Console.WriteLine(x);
            Console.WriteLine(t);
            Console.WriteLine($"Count:{count} time {stop.ElapsedMilliseconds}");

            #endregion

            Console.ReadLine();


        }

       
    }
}
