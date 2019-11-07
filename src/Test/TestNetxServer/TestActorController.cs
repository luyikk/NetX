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
    public class TestActorController:ActorController,IActorService
    {
        public ILog Log { get; }

        /// <summary>
        /// 构造函数可以写,可以不写,可以传入日记和你想要的Instance,
        /// Instence可以Build 的时候调用RegisterDescriptors 自定义Instence
        /// </summary>
        /// <param name="logger"></param>
        public TestActorController(ILogger<TestController> logger)
        {
            Log = new DefaultLog(logger);
        }

               
        /// <summary>
        /// 我们用来记录被调用了多少次
        /// </summary>
        public int UseCount { get; private set; }


        //TAG 标记是用来区分功能函数的 唯一标签,他永远比函数名称重要,函数名称可以随便写,但是TAG和参数不能出错
      
        public  Task<int> Add(int a, int b)
        {         
            UseCount++;
            return Task.FromResult(a + b);
        }


        [Open(OpenAccess.Internal)]       
        public Task<string> GetData()
        {
            return Task.FromResult("123123");
        }

      
        public async void Show(string msg)
        {
            //我们可以通过这样的方法去调用其他的Actor功能,他是安全的,可以随便用,这个地方等于他调用了自己的.
            //虽然可以这么写,当然我们还是 直接调用函数比较好,因为那样子更快
            var c = await Get<IActorService>().Add(1, 2);
            Log.Info(c);
            UseCount++;
            Log.Info(msg);
            
        }

    }


}
