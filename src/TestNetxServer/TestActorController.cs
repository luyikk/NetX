using Microsoft.Extensions.Logging;
using Netx;
using Netx.Actor;
using Netx.Loggine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestNetxServer
{
    public class TestActorController:ActorController
    {
        public ILog Log { get; }

        public TestActorController(ILogger<TestController> logger)
        {
            Log = new DefaultLog(logger);
        }

               

        [TAG(2000)]
        public  Task<int> Add(int a, int b)
        {
            return Task.FromResult(a + b);
        }

        [TAG(3000)]
        public void Run(string msg)
        {
            Log.Info(msg);
        }
    }
}
