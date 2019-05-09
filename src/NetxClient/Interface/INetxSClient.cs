using System.Threading.Tasks;
using ZYSocket.Client;

namespace Netx
{
    public interface INetxSClient
    {
      
        T Get<T>();

        void Action(int cmdTag, params object[] args);
        Task AsyncAction(int cmdTag, params object[] args);
        Task<IResult> AsyncFunc(int cmdTag, params object[] args);

        void LoadInstance(object instance);
        void RemoveInstance(object instacne);        

    }
}