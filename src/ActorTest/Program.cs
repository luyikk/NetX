using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx;
using Netx.Actor;
using Netx.Interface;
using System;
using System.Threading.Tasks;

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
            Container.AddLogging(p =>
            {
                p.AddConsole();
                p.SetMinimumLevel(LogLevel.Trace);
            });

            var build= Container.BuildServiceProvider();

            var Actor=  build.GetRequiredService<ActorRun>();
       
            var server= Actor.Get<ICallServer>();


           var stop = System.Diagnostics.Stopwatch.StartNew();

            var x = 0;

            //Parallel.For(0, 1000000, async i =>
            //  {
            //      x = await server.Add(i, x);

            //  });



            for (int i = 0; i < 1000000; i++)
            {
                x = await server.Add(i, x);
            }


            var t = await server.GetV();           
            stop.Stop();

            Console.WriteLine(x);
            Console.WriteLine(t);
            Console.WriteLine("time :" + stop.ElapsedMilliseconds);
         
            Console.ReadLine();
        }

      
    }
}
