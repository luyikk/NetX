using Interfaces;

using Netx;
using Netx.Service;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Client;

namespace Server
{
    public class MyAsyncController : AsyncController, IServer,IDisposable  ,IServerNew 
    {
        public Task<int> Add(int a, int b)
        {
            return Get<IClient>().Add(a, b);
        }

        public Task<int> AddOne(int a)
        {
            return Get<IClient>().AddOne(a);
        }

        public Task<int> AddTow(int a)
        {
            return Get<IClientNew>().AddTow(a);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Run(string msg)
        {
            Console.WriteLine(msg);
        }

        [TAG(2000)]
        public Task<int> Add3(int a)
        {
            return Task.FromResult(a +3);
        }

        public Task<int> Sub(int a, int b)
        {
            return Task.FromResult(a - b);
        }
    }
}
