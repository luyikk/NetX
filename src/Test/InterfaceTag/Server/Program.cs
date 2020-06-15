using System;
using System.Reflection;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new Netx.Service.Builder.NetxServBuilder()
             .ConfigBase(p =>
             {
                 p.VerifyKey = "123123";
                 p.ClearSessionTime = 5000;
             })
             .RegisterService(Assembly.GetExecutingAssembly()) //注册当前DLL下面的所有控制器,包含RPC控制器和ACTOR控制器,我们都定义了一个
             .ConfigNetWork(p => //配置服务器SOCKET 方面的
                {
                    p.MaxConnectCout = 100;
                    p.Port = 3000;

                })
             .Build();
            service.Start(); //开始服务

            Console.ReadLine();
        }
    }
}
