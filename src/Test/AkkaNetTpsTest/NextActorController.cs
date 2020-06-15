using Microsoft.Extensions.Logging;
using Netx.Actor;
using Netx.Loggine;
using System.Threading.Tasks;

namespace AkkaNetTpsTest
{
    /// <summary>
    /// 整个Actor 容器为全局唯一
    /// </summary>

    public class NextActorController : ActorController, INetxServer
    {
        public ILog Log { get; }

        public NextActorController(ILogger<NextActorController> logger)
        {
            Log = new DefaultLog(logger);
        }

        private int x = 0;


        public Task<int> Income(int a)
        {
            x += a;
            return Task.FromResult(x);
        }

        public Task<int> Payout(int a)
        {
            x -= a;
            return Task.FromResult(x);
        }

        public Task<int> Amount()
        {
            return Task.FromResult(x);
        }
    }
}
