using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Netx.Actor
{

    public class LambdaController : ActorController, IActorLambda
    {
        public Task Ask(Action action)
        {
            action?.Invoke();
            return Task.CompletedTask;
        }

        public Task<dynamic> Ask(Func<dynamic> func)
        {
            return Task.FromResult<dynamic>(func.Invoke());
        }

        public void Tell(Action action)
        {
            action?.Invoke();
        }
    }
}
