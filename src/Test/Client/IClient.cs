using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    [Build]
    public interface IClient
    {
        [TAG(100)]
        Task<int> Add(int a, int b);
    }
}
