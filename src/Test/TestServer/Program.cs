using System;
using System.Reflection;

namespace TestServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new Netx.Service.Builder.NetxServBuilder()
                .RegisterService(Assembly.GetExecutingAssembly())
                .ConfigNetWork(p =>
                {
                    p.MaxConnectCout = 100;
                    p.Port = 50054;
                })
                .ConfigObjFormat(() => new NetxSerializes.JSONSerializes())
                .Build();

            service.Start();

            Console.ReadLine();
        }
    }
}
