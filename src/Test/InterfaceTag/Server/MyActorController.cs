using Interfaces;
using Netx;
using Netx.Actor;
using System;
using System.Threading.Tasks;

namespace Server
{
    [ActorOption(1000, 10000)]
    public class MyActorController : ActorController, IActors, IActorsNew, IActorsSub
    {
        public Task<int> Add(int a, int b)
        {
            return Task.FromResult(a + b);
        }

        public Task<int> AddOne(int a)
        {
            return Task.FromResult(++a);
        }


        public Task<int> AddTow(int a)
        {
            return Task.FromResult(a + 2);
        }

        public void Run(string msg)
        {
            Console.WriteLine(msg);
        }

        [TAG(20001)]
        public Task<int> Add3(int a)
        {
            return Task.FromResult(a + 3);
        }

        [Open(OpenAccess.Internal)]
        public Task<int> Sub(int a, int b)
        {
            return Task.FromResult(a - b);
        }


    }
}
