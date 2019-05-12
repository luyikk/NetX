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


        [TAG(1003)]
        public Task<int> ToClientAddOne(int a)
        {
            return Get<IClientCalling>().AddOne(a);
        }


        [TAG(1004)]
        public Task<int> ToClientAdd(int a,int b)
        {
            return Get<IClientCalling>().Add(a,b);
        }

        /// <summary>
        /// 递归测试
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        [TAG(1005)]
        public Task<int> RecursiveTest(int a)
        {
            a--;
            if (a > 0)
                return Get<IClientCalling>().Recursive(a);
            else
                return Task.FromResult(a);
        }

        [TAG(1007)]
        public async Task<bool> TestTimeOut()
        {
            await Task.Delay(11000);
            return true;
        }



        [TAG(1006)]
        public void Finsh()
        {
            Log.Info("the token finsh,disconnect it");
            this.Current.DisconnectIt();
          
        }

        public override void Disconnect()
        {
            Log.Info("disconnect");
        }

        public override void Closed()
        {
            Log.Info("closed");
        }
    }
}
