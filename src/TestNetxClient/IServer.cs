using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestNetxClient
{
    [Build]
    interface IServer
    {
        [TAG(2000)]
        Task<int> AddActor(int a, int b);

        [TAG(1000)]
        Task<int> Add(int a, int b);

        [TAG(3000)]
        void RunMsg();
    }
}
