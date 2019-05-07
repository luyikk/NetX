using Microsoft.Extensions.Logging;
using Netx;
using Netx.Actor;
using Netx.Loggine;
using System.Threading.Tasks;

namespace ActorTest
{
    public class NextActorController : ActorController
    {
        public ILog Log { get; }

        public NextActorController(ILogger<TestActorController> logger)
        {
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

    
    }
}
