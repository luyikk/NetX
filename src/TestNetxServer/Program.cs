using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TestNetxServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new Netx.Service.Builder.NetxServBuilder()
                .AddActorEvent<ActorEvent1>() //添加绑定事件1
                .AddActorEvent<ActorEvent2>() //添加绑定事件2
                .RegisterService(Assembly.GetExecutingAssembly()) //注册当前DLL下面的所有控制器,包含RPC控制器和ACTOR控制器,我们都定义了一个
                .ConfigServer(p => //配置服务器SOCKET 方面的
                {
                    p.MaxConnectCout = 100;
                    p.Port = 1006;

                })        
                .Build();

          
            service.Start(); //开始服务

            Console.ReadLine();
        }
    }
}
