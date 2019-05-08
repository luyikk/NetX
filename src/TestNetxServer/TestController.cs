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
    /// <summary>
    /// RPC 服务
    /// </summary>
    public class TestController : AsyncController
    {
        public ILog Log { get; }

        public TestController(ILogger<TestController> logger)
        {
            Log = new DefaultLog(logger);
        }
        /// <summary>
        /// 加法计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [TAG(1000)]
        public  Task<int> Add(int a,int b)
        {
            return Task.FromResult(a + b);
        }

        /// <summary>
        /// 去调用ACTOR 加法计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [TAG(1001)]
        public Task<int> Add2(int a,int b)
        {
            //测试 去调用ACTOR 的ADD 
            return Actor<IActorService>().Add(a, b);
        }
    }
}
