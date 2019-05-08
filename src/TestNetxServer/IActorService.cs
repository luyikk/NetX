using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestNetxServer
{
    [Build]
    public interface IActorService
    {
        [TAG(2000)]
        Task<int> Add(int a, int b);
    }
}
