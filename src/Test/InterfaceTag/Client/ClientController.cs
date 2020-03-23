using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientController : IMethodController,IClient,IClientNew
    {
        public INetxSClientBase Current { get; set; }

        public T Get<T>() => Current.Get<T>();



        public Task<int> Add(int a, int b)
        {
            return Task.FromResult(a + b);
        }

        public Task<int> AddOne(int a)
        {
            return Task.FromResult(++a);
        }

        public Task<int> AddTow(int a)
        {
            return Task.FromResult(a+2);
        }

      



    }
}
