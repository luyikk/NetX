using Microsoft.Extensions.Logging;
using Netx;
using Netx.Actor;
using Netx.Loggine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ActorTest
{
    public class TestActorController:ActorController
    {
        public ILog Log { get; }

        public TestActorController(ILogger<TestActorController> logger)
        {
            Log = new DefaultLog(logger);
        }

        int xa = 0;
               

        [TAG(2000)]
        public Task<int> Add(int a, int b)
        {
            xa+= a + b;
            return Task.FromResult(a + b);
        }

        [TAG(2001)]
        public Task<int> GetV()
        {           
            return Task.FromResult(xa);
        }
    }
}
