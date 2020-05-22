using System;
using System.Threading.Tasks;

namespace Netx.Actor
{
    enum LambdaFuncEnum
    {
        TellArgs = -1999990111,
        Tell = -1999990112,
        Ask = -1999990113,     
        Func= -1999990114,
        AskArg = -1999990115,
        FuncArg = -1999990116,
    };

    [Build]
    public interface IActorLambda
    {
        [TAG(LambdaFuncEnum.TellArgs)]
        void Tell(Action<dynamic> action, dynamic arg);

        [TAG(LambdaFuncEnum.Tell)]
        void Tell(Action action);

        [TAG(LambdaFuncEnum.Ask)]
        Task Ask(Action action);

        [TAG(LambdaFuncEnum.AskArg)]
        Task Ask(Action<dynamic> action, dynamic arg);

        [TAG(LambdaFuncEnum.Func)]
        Task<dynamic> Ask(Func<dynamic> func);

        [TAG(LambdaFuncEnum.FuncArg)]
        Task<dynamic> Ask(Func<dynamic, dynamic> func, dynamic arg);
    }
}
