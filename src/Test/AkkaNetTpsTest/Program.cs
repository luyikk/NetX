using Akka.Actor;
using AkkaNetTpsTest;
using EventNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx;
using Netx.Actor;
using Netx.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventNext_AkkaNet
{
    class Program
    {
        private static EventCenter EventCenter = new EventCenter();

        private static IServiceProvider NetXActorSP;

        private static ActorSystem Akka;

        private static IActorRef henryActor;

        private static IActorRef nbActor;

        private static int mCount;

        private static int[] mConcurrent = new int[] { 10, 20, 50, 100, 200, 500 };

        private static int mFors = 10000;

        static void Main(string[] args)
        {

           

         
           
            var actor = new Netx.Actor.Builder.ActorBuilder();
            actor.RegisterService<NextActorController>();
            actor.Build();


            var x = System.Runtime.InteropServices.Marshal.SizeOf<Netx.Actor.Builder.ActorBuilder>(actor);


            NetXActorSP = actor.Provider;

            EventCenter.Register(typeof(Program).Assembly);
            EventCenter.LogOutput += (o, e) =>
            {
                Console.WriteLine($"[{e.Type}]{e.Message}");
            };
            Akka = ActorSystem.Create("MySystem");
            henryActor = Akka.ActorOf<UserActor>("henry");
            nbActor = Akka.ActorOf<UserActor>("nb");
            Test();
            Console.Read();
        }

        static async void Test()
        {
            foreach (var c in mConcurrent)
            {
                await AkkaTest(c);
                await NetxTest(c);
                await EventNextTest(c);
               
            }
        }

        static async Task NetxTest(int concurrent)
        {
            mCount = 0;

            var NetXActor1 = NetXActorSP.GetRequiredService<ActorRun>();
            var NetXActor2 = NetXActorSP.CreateScope().ServiceProvider.GetRequiredService<ActorRun>();

            var server1 = NetXActor1.Get<INetxServer>();
            var server2 = NetXActor2.Get<INetxServer>();

            long start = EventCenter.Watch.ElapsedMilliseconds;         

            List<Task> tasks = new List<Task>();
            for (int k = 0; k < concurrent; k++)
            {
                var task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        //await NetXActor1.CallFunc<int>(i, 1, OpenAccess.Internal, i);
                        await server1.Income(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                         //await NetXActor1.CallFunc<int>(i, 2, OpenAccess.Internal, i);
                        await server1.Payout(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);


                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        //await NetXActor2.CallFunc<int>(i, 1, OpenAccess.Internal, i);
                        await server2.Income(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                         //await NetXActor2.CallFunc<int>(i, 2, OpenAccess.Internal, i);
                        await server2.Payout(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);


            }
            Task.WaitAll(tasks.ToArray());

            var henryAmount = await server2.Amount();
            var nbAmount = await server1.Amount();
            double usetime = EventCenter.Watch.ElapsedMilliseconds - start;
            Console.WriteLine($"NetX concurrent{concurrent}|use time:{EventCenter.Watch.ElapsedMilliseconds - start}|count:{mCount}|rps:{(mCount / usetime * 1000):####}|henry:{nbAmount}|nb:{nbAmount}");
        }

        static async Task AkkaTest(int concurrent)
        {
            mCount = 0;
            long start = EventCenter.Watch.ElapsedMilliseconds;
            List<Task> tasks = new List<Task>();
            for (int k = 0; k < concurrent; k++)
            {
                var task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        Income income = new Income { Memory = i };
                        var result = await henryActor.Ask<decimal>(income);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        Payout payout = new Payout { Memory = i };
                        var result = await henryActor.Ask<decimal>(payout);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);


                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        Income income = new Income { Memory = i };
                        var result = await nbActor.Ask<decimal>(income);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        Payout payout = new Payout { Memory = i };
                        var result = await nbActor.Ask<decimal>(payout);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);


            }
            Task.WaitAll(tasks.ToArray());
            var get = new Get();
            var henryAmount = await henryActor.Ask<decimal>(get);
            var nbAmount = await nbActor.Ask<decimal>(get);
            double usetime = EventCenter.Watch.ElapsedMilliseconds - start;
            Console.WriteLine($"Akka concurrent{concurrent}|use time:{EventCenter.Watch.ElapsedMilliseconds - start}|count:{mCount}|rps:{(mCount / usetime * 1000):####}|henry:{henryAmount}|nb:{nbAmount}");
        }

        static async Task EventNextTest(int concurrent)
        {
            mCount = 0;
            IUserService henry = EventCenter.Create<IUserService>("henry"); ;
            IUserService nb = EventCenter.Create<IUserService>("nb"); ;
            long start = EventCenter.Watch.ElapsedMilliseconds;
            List<Task> tasks = new List<Task>();
            for (int k = 0; k < concurrent; k++)
            {
                var task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        var result = await henry.Income(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        var result = await henry.Payout(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);


                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        var result = await nb.Income(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);

                task = Task.Run(async () =>
                {
                    for (int i = 0; i < mFors; i++)
                    {
                        var result = await nb.Payout(i);
                        System.Threading.Interlocked.Increment(ref mCount);
                    }
                });
                tasks.Add(task);


            }
            Task.WaitAll(tasks.ToArray());
            var henryAmount = await henry.Amount();
            var nbAmount = await nb.Amount();
            double usetime = EventCenter.Watch.ElapsedMilliseconds - start;
            Console.WriteLine($"EventNext concurrent{concurrent} use time:{usetime}|count:{mCount}|rps:{(mCount / usetime * 1000):####}|henry:{henryAmount}|nb:{nbAmount}");
        }
    }
}
