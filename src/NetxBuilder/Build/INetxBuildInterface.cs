using System;
using System.Threading.Tasks;

namespace Netx
{

    public interface INetxBuildInterface
    {
        Task<IResult> AsyncFunc(int cmdtag, params object[] args);
        Task<T> AsyncFunc<T>(int cmdtag, params object[] args);
        Task AsyncAction(int cmdTag, params object[] args);
        void Action(int cmdTag, params object[] args);
        object Func(int cmdTag, Type type, params object[] args);
    }
}
