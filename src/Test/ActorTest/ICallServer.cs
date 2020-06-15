using Netx;
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

        [TAG(10001)]
        Task<User> GetUser(int id);

        [TAG(10002)]
        Task<bool> AddUserCoin(int Id, int coin);

        [TAG(10003)]
        Task<bool> SubUserCoin(int Id, int coin);

        [TAG(10004)]
        Task<bool> SetUserCoin(int Id, int coin);

        [TAG(10005)]
        Task TestWait();

        [TAG(10006)]
        void TestWrite(string msg);


        [TAG(20000)]
        Task<int> TestWait(int a);
    }
}
