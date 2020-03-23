using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Netx;
namespace TestClient
{
    [Build]
    public interface IServer
    {
        [TAG(999)]
        Task<int> AddOne(int a);


        [TAG(1005)]
        void PrintMsg(string msg);


        [TAG(2000)]
        Task<int> AddOneActor(int a);

        [TAG(2500)]
        Task<long> GetAllCount();


    }
}

