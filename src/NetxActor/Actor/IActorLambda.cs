using System;
using System.Threading.Tasks;

namespace Netx.Actor
{
    enum LambdaFuncEnum
    {
        Tell = -1999990112,
        Ask = -1999990113,     
        Func= -1999990114,
    };

    [Build]
    public interface IActorLambda
    {
        [TAG(LambdaFuncEnum.Tell)]
        void Tell(Action action);

        [TAG(LambdaFuncEnum.Ask)]
        Task Ask(Action action);

        [TAG(LambdaFuncEnum.Func)]
        Task<dynamic> Ask(Func<dynamic> func);
    }
}
