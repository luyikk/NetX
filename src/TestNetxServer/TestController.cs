using Microsoft.Extensions.Logging;
using Netx;
using Netx.Loggine;
using Netx.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestNetxServer
{
    public class TestController : AsyncController
    {
        public ILog Log { get; }

        public TestController(ILogger<TestController> logger)
        {
            Log = new DefaultLog(logger);
        }

        [TAG(1000)]
        public  Task<int> Add(int a,int b)
        {
            return Task.FromResult(a + b);
        }
    }
}
