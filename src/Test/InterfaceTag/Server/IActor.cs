using Netx;
using System.Threading.Tasks;

namespace Interfaces
{
    [Build]
    public interface IActorsSub
    {
        [TAG(10010)]
        Task<int> Sub(int a, int b);
    }

    [Build]
    public interface IActorsOld
    {
        [TAG(10001)]
        Task<int> AddOne(int a);
    }

    [Build]
    public interface IActors : IActorsOld
    {

        [TAG(10000)]
        Task<int> Add(int a, int b);
    }

    [Build]
    public interface IActorsNew
    {
        [TAG(10002)]
        Task<int> AddTow(int a);

        [TAG(10003)]
        void Run(string msg);
    }
}
