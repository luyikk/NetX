using Netx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Client
{

    [Build]
    public interface IClientOld
    {
        [TAG(1000)]
        Task<int> AddOne(int a);
    }

    [Build]
    public interface IClientNew
    {
        [TAG(1002)]
        Task<int> AddTow(int a);
    }


    [Build]
    public interface IClient:IClientOld
    {
        [TAG(1001)]
        Task<int> Add(int a, int b);
    }
}
