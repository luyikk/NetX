using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ZYSocket.Client;

namespace Netx
{
    public interface INetxSClient
    {
        IServiceProvider Container { get; }
        T Get<T>();
        ILogger GetLogger(string categoryName);
        void Action(int cmdTag, params object[] args);
        Task AsyncAction(int cmdTag, params object[] args);
        Task<IResult> AsyncFunc(int cmdTag, params object[] args);
        void LoadInstance(object instance);
        void RemoveInstance(object instacne);        

    }
}