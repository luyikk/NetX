using Microsoft.Extensions.Logging;
using Netx;
using Netx.Actor;
using Netx.Loggine;
using System.Threading.Tasks;

namespace ActorTest
{
    [ActorOption(maxQueueCount: 1000, ideltime: 3000)]
    public class TestActorController : ActorController
    {
        public ILog Log { get; }

        public TestActorController(ILogger<TestActorController> logger)
        {
            Log = new DefaultLog(logger);
        }

        public long xa { get; private set; }


        [TAG(2000)]
        public Task<int> Add(int a, int b)
        {

            xa++;
            return Task.FromResult(a + b);
        }

        [TAG(2001)]
        public Task<long> GetV()
        {
            return Task.FromResult(xa);
        }


        [TAG(2010)]
        public async Task<int> Add2(int a, int b)
        {
            var x = await Get<ICallServer>().AddX();

            xa = a + b - x;
            return a + b;
        }

        [TAG(20000)]
        public async Task<int> TestWait(int a)
        {
            await Task.Delay(100);
            return a;
        }

    }
}
