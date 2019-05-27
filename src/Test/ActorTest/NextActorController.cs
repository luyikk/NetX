using Microsoft.Extensions.Logging;
using Netx;
using Netx.Actor;
using Netx.Loggine;
using System.Threading.Tasks;
using ZYSQL;

namespace ActorTest
{
    /// <summary>
    /// 整个Actor 容器为全局唯一
    /// </summary>
    [ActorOption(maxQueueCount:1000, ideltime: 10000)] //设置最大列队数1000, 设置多久没有订单后进入休眠状态
    public class NextActorController : ActorController
    {
        public ILog Log { get; }

        public NextActorController(ILogger<TestActorController> logger,int a)
        {

            ZYSQL.SqlInstance.Instance.InstallConfig(new DataConnectConfig[]
            {
                new DataConnectConfig()
                {
                    Name="DefautConnectionString",
                    ConnectionString="Data Source=|DataDirectory|./Test.db3;Pooling=true;FailIfMissing=false",
                    SqlType="SQLite",
                    MaxCount=100
                }
            });


            Log = new DefaultLog(logger);
        }

        public int x { get; private set; }

        [TAG(3001)]
        public Task<int> GetX()
        {          
            return Task.FromResult(x);
        }

        [TAG(3002)]
        public Task<int> AddX()
        {
            x++;
            return Task.FromResult(x);
        }




        [TAG(10001)]
        public  Task<User> GetUser(int Id)
        {
            using (ZYSQL.SQLiteExecuteXY obj = new SQLiteExecuteXY())
            {
               return  obj.SqlExcuteSelectFirstAsync<User>("SELECT * FROM Users WHERE Id=@Id", new System.Data.SQLite.SQLiteParameter("@Id", Id));              
            }
        }

        [TAG(10002)]
        public async Task<bool> AddUserCoin(int Id,int coin)
        {
            var user = await GetUser(Id);
            user.Coin += coin;

            using (ZYSQL.SQLiteExecuteXY obj = new SQLiteExecuteXY())
            {
                if (await obj.SqlExcuteUpdateOrInsertOrDeleteObjectAsync("UPDATE Users Set Coin=@Coin WHERE Id=@Id",user)==1)
                    return true;

                return false;
            }

        }


        [TAG(10003)]
        public async Task<bool> SubUserCoin(int Id, int coin)
        {
            var user = await GetUser(Id);
            user.Coin -= coin;

            using (ZYSQL.SQLiteExecuteXY obj = new SQLiteExecuteXY())
            {
                if (await obj.SqlExcuteUpdateOrInsertOrDeleteObjectAsync("UPDATE Users Set Coin=@Coin WHERE Id=@Id", user) == 1)
                    return true;

                return false;
            }

        }


        [TAG(10004)]
        public async Task<bool> SetUserCoin(int Id, int coin)
        {
            var user = await GetUser(Id);
            user.Coin = coin;

            using (ZYSQL.SQLiteExecuteXY obj = new SQLiteExecuteXY())
            {
                if (await obj.SqlExcuteUpdateOrInsertOrDeleteObjectAsync("UPDATE Users Set Coin=@Coin WHERE Id=@Id", user) == 1)
                    return true;

                return false;
            }

        }

        public override Task Sleeping()
        {
            Log.Info("Save Data ing..");

            return Task.CompletedTask;
        }

        public override Task Awakening()
        {
            Log.Info("Read Data ing..");

            return Task.CompletedTask;
        }
    }
}
