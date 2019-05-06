using Microsoft.Extensions.Logging;
using Netx;
using Netx.Actor;
using Netx.Loggine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TPSTestServer
{
    public  class TestActorController:ActorController
    {
        public ILog Log { get; }

        public TestActorController(ILogger<TestActorController> logger)
        {
            Log = new DefaultLog(logger);
        }

        long i = 0;

        [TAG(2000)]
        public Task<int> AddOne(int a)
        {
            i++;
            return Task.FromResult(++a);
        }

        [TAG(2500)]
        public Task<long> GetCount()
        {
            return Task.FromResult(i);
        }

    }
}
