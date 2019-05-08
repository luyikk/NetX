using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ActorTest
{
    [Build]
    interface ICallServer
    {
        [TAG(2000)]
        Task<int> Add(int a, int b);

        [TAG(2001)]
        Task<long> GetV();

        [TAG(3001)]
        Task<int> GetX();

        [TAG(2010)]
        Task<int> Add2(int a, int b);

        [TAG(3002)]
        Task<int> AddX();
    }
}
