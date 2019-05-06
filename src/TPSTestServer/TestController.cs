using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Netx;
using Netx.Loggine;
using Netx.Service;

namespace TestServer
{
    public class TestController: AsyncController
    {
     
        public ILog Log { get; }

        public TestController(ILogger<TestController> logger)
        {
            Log = new DefaultLog(logger);         
        }


        [TAG(999)]
        public Task<int> Add(int a)
        {
           
            return Task.FromResult<int>(++a);
        }

        [TAG(1005)]
        public void Message(string msg)
        {
            Log.Info(msg);
        }
    }
}
