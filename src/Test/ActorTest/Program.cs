using Microsoft.Extensions.DependencyInjection;
using Netx.Actor;
using Netx.Actor.Builder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ActorTest
{
    class Program
    {
        static IServiceCollection Container = new ServiceCollection();
        static async Task Main(string[] args)
        {

            var Actor = new ActorBuilder()
                 .UseActorLambda()
                 //.ConfigureActorScheduler(p=>ActorScheduler.TaskFactory)
                 .RegisterService<TestActorController>()
                 .RegisterService<NextActorController>().Build();


            #region use akka model

            var lambda = Actor.Get<IActorLambda>();
            {
                int icount = 0;

                List<Task> waitlist = new List<Task>();

                for (int i = 0; i < 10000; i++)
                {
                    waitlist.Add(Task.Factory.StartNew((p) =>
                    {
                        lambda.Tell(() =>
                        {
                            icount += (int)p;
                        });

                    }, i));
                }


                for (int i = 0; i < 10000; i++)
                {
                    waitlist.Add(Task.Factory.StartNew((p) =>
                    {
                        lambda.Tell(() =>
                        {
                            icount -= (int)p;
                        });
                    }, i));
                }


                await Task.WhenAll(waitlist);

                Debug.Assert(icount == 0);
                Console.WriteLine($"tell:{icount}");
            }
            {
                int icount = 0;

                List<Task> waitlist = new List<Task>();


                waitlist.Add(Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        lambda.Tell((p) =>
                        {
                            icount += p;
                        }, i);
                    }
                }));


                waitlist.Add(Task.Factory.StartNew(() =>
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        lambda.Tell((p) =>
                        {
                            icount -= p;
                        }, i);
                    }
                }));


                waitlist.Add(Task.Factory.StartNew(async () =>
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        await lambda.Ask((p) =>
                        {
                            icount += p;
                        }, i);
                    }
                }));


                waitlist.Add(Task.Factory.StartNew(async () =>
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        await lambda.Ask((p) =>
                        {
                            icount -= p;
                        }, i);
                    }
                }));




                await Task.WhenAll(waitlist);

                Debug.Assert(icount == 0);
                Console.WriteLine($"ask:{icount}");
            }
            {
                int icount = 0;

                List<Task> waitlist = new List<Task>();
                for (int i = 0; i < 10000; i++)
                {
                    waitlist.Add(Task.Factory.StartNew(async (p) =>
                    {
                        var res = await lambda.Ask((c) =>
                        {
                            icount -= c;
                            return p;
                        }, p);

                    }, i));
                }

                for (int i = 0; i < 10000; i++)
                {
                    waitlist.Add(Task.Factory.StartNew(async (p) =>
                    {
                        var res = await lambda.Ask((c) =>
                        {
                            icount += c;
                            return p;
                        }, p);
                    }, i));
                }

                await Task.WhenAll(waitlist);

                Debug.Assert(icount == 0);

                Console.WriteLine($"ask:{icount}");
            }
            {
                int icount = 0;

                List<Task> waitlist = new List<Task>();
                for (int i = 0; i < 10000; i++)
                {
                    waitlist.Add(Task.Factory.StartNew(async (p) =>
                     {
                         await lambda.Ask(() =>
                         {
                             icount += (int)p;
                         });

                     }, i));
                }

                for (int i = 0; i < 10000; i++)
                {
                    waitlist.Add(Task.Factory.StartNew(async (p) =>
                      {
                          await lambda.Ask(() =>
                          {
                              icount -= (int)p;
                          });

                      }, i));
                }

                await Task.WhenAll(waitlist);

                Debug.Assert(icount == 0);

                Console.WriteLine($"ask:{icount}");
            }

            #endregion

            #region testsql
            var server = Actor.Get<ICallServer>();

            await server.Add(0, 0);

            await server.SetUserCoin(1, 100);
            var user = await server.GetUser(1);
            Console.WriteLine($"{user.Name} current coin:{user.Coin}");


            var task1 = Task.Run(() =>
              {
                  for (int i = 0; i < 10; i++)
                  {
                      server.AddUserCoin(1, 100);
                  }

              });

            var task2 = Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    server.SubUserCoin(1, 100);
                }

            });

            await Task.WhenAll(task1, task2);

            user = await server.GetUser(1);
            Console.WriteLine($"{user.Name} current coin:{user.Coin}");

            await server.TestWait();
            server.TestWrite($"{user.Name} coin is Reset");
            await server.SetUserCoin(1, 100);

            #endregion

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
                x = await Actor.CallFunc<int>(i, 2000, OpenAccess.Internal, i, x);
                count++;
            }

            stop.Stop();

            t = await server.GetV();
            Console.WriteLine(x);
            Console.WriteLine(t);
            Console.WriteLine($"Count:{count} time {stop.ElapsedMilliseconds}");


            stop.Restart();

            x = 0;
            count = 0;

            for (int i = 0; i < 2000000; i++)
            {
                await lambda.Ask(() => { x += i; });
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
