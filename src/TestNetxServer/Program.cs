﻿using Microsoft.Extensions.DependencyInjection;
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
                .RegisterService(Assembly.GetExecutingAssembly())                         
                .ConfigServer(p =>
                {
                    p.MaxConnectCout = 100;
                    p.Port = 1006;

                })        
                .Build();

            service.Start();

            Console.ReadLine();
        }
    }
}