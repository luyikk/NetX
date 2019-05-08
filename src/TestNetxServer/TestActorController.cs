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
    [ActorOption(1000)] //限制最大列队数为1000,不设置此标签表示不限制
    public class TestActorController:ActorController
    {
        public ILog Log { get; }

        public TestActorController(ILogger<TestController> logger)
        {
            Log = new DefaultLog(logger);
        }

               
        public int UseCount { get; private set; }

        [TAG(2000)]
        public  Task<int> Add(int a, int b)
        {         
            UseCount++;
            return Task.FromResult(a + b);
        }

        [TAG(3000)]
        public void Run(string msg)
        {
            UseCount++;
            Log.Info(msg);
        }
    }
}
