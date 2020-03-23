using Microsoft.Extensions.Logging;
using Netx.Loggine;
using System;
using System.Threading.Tasks;
using ZYSocket.Client;

namespace Netx
{


    public interface INetxSClientBase
    {
      
        IServiceProvider Container { get; }
        ILog Log { get; }
        T Get<T>();
        ILogger GetLogger<T>();        
        void Action(int cmdTag, params object[] args);
        Task AsyncAction(int cmdTag, params object[] args);
        Task<IResult> AsyncFunc(int cmdTag, params object[] args);
        void LoadInstance(object instance);
        void RemoveInstance(object instacne);
    }

    public interface INetxSClient: INetxSClientBase,IDisposable
    {
        public event DisconnectHandler? Disconnect;
        void Open();
        Task<ConnectResult> OpenAsync();
        void Close(); 
    }
}