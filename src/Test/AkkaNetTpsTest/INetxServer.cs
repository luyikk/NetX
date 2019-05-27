using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AkkaNetTpsTest
{
    [Build]
    public interface INetxServer
    {
        [TAG(1)]
        Task<int> Income(int a);

        [TAG(2)]
        Task<int> Payout(int a);

        [TAG(3)]
        Task<int> Amount();
    }
}
