using Netx;
using System;
using System.Threading.Tasks;

namespace Interfaces
{
    [Build]
    public interface IServerOld
    {
        [TAG(1001)]
        Task<int> AddOne(int a);
    }

    [Build]
    public interface IServer: IServerOld
    {
        [TAG(1000)]
        Task<int> Add(int a, int b);
    }

    [Build]
    public interface IServerNew
    {
        [TAG(1002)]
        Task<int> AddTow(int a);

        [TAG(1003)]
        void Run(string msg);
    }

    [Build]
    public interface IServerDef
    {
        [TAG(2000)]
        Task<int> Add3(int a);
    }
}
