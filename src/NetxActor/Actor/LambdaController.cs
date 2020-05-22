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

        public Task Ask(Action<dynamic> action, dynamic arg)
        {
            action?.Invoke(arg);
            return Task.CompletedTask;
        }

        public Task<dynamic> Ask(Func<dynamic, dynamic> func, dynamic arg)
        {
            return Task.FromResult<dynamic>(func.Invoke(arg));
        }

        public void Tell(Action action)
        {
            action?.Invoke();
        }

        public void Tell(Action<dynamic> action, dynamic arg)
        {
            action?.Invoke(arg);
        }
    }
}
