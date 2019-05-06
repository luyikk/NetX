using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestNetxClient
{
    public class ClientTestController
    {
        [TAG(2000)]
        public  Task<int> AddOne(int a)
        {
            return Task.FromResult(++a);
        }

        [TAG(2001)]
        public Task<int> Add(int a,int b)
        {
            return Task.FromResult(a+b);
        }
    }
}
