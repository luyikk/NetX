using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ZYSocket.FiberStream;
using ZYSocket;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Netx.Service
{
    public abstract class ServiceToken:ServiceInstall
    {

        private readonly Lazy<ServiceTokenFactory> LazyServiceTokenFactory;
        protected ServiceTokenFactory TokenFactory { get => LazyServiceTokenFactory.Value; }

        protected ConcurrentDictionary<long, AsyncToken> ActorTokenDict { get; }    
    
        private readonly Lazy<ConcurrentQueue<TimeKey>> disconnectRemoveList;
        private ConcurrentQueue<TimeKey> DisconnectRemoveList { get => disconnectRemoveList.Value; }




        private struct TimeKey
        {
            public long Key { get; }
            public long Time { get; }
            public TimeKey(long key, long time)
            {
                Key = key;
                Time = time;
            }
        }


        public ServiceToken(IServiceProvider container) :base(container)
        {
            ActorTokenDict = new ConcurrentDictionary<long, AsyncToken>();
            LazyServiceTokenFactory = new Lazy<ServiceTokenFactory>(() => new ServiceTokenFactory(container),true);
            disconnectRemoveList = new Lazy<ConcurrentQueue<TimeKey>>();
          
            ServiceOption.ClearCheckTime = ServiceOption.ClearCheckTime <= 100 ? 100 : ServiceOption.ClearCheckTime;

            Task.Factory.StartNew(RemoveRun);
        }

        private async void RemoveRun()
        {
            while (true)
            {
                await Task.Delay(ServiceOption.ClearCheckTime);

                if (ServiceOption.ClearSessionTime > 0)
                    CheckSessionTimeOut();

                if (ServiceOption.ClearRequestTime > 0)
                    CheckRequestTimeOut();
            }
        }

        /// <summary>
        /// 检测Request超时
        /// </summary>
        private void CheckRequestTimeOut()
        {
            
            foreach (var token in ActorTokenDict.Values)
                token.RequestTimeOutHandle();
        }



        /// <summary>
        /// 检测Session超时
        /// </summary>
        private void CheckSessionTimeOut()
        {
            var outtime = ServiceOption.ClearSessionTime * 10000;

            while (DisconnectRemoveList.Count>0)
            {
                if (DisconnectRemoveList.TryPeek(out TimeKey tv))
                {
                    if ((TimeHelper.GetTime() - tv.Time) > outtime)
                    {
                        if(DisconnectRemoveList.TryDequeue(out TimeKey timeKey))
                        {
                            if (ActorTokenDict.TryGetValue(timeKey.Key, out AsyncToken token))
                            {
                                if (!token.IsConnect)
                                    if(ActorTokenDict.TryRemove(timeKey.Key, out AsyncToken _))
                                    {
                                        token.Close();
                                        Log.Trace($"session:{ token.SessionId} remove");
                                    }
                            }
                        }
                    }
                    else
                        break;
                }
                else
                    break;
            }          
        }


        protected async Task<bool> RunCreateToken(IFiberRw<AsyncToken> fiberRw)
        {
            var token = TokenFactory.CreateAsynToken(fiberRw,AsyncServicesRegisterDict);

            if (!ActorTokenDict.TryAdd(token.SessionId, token))
                ActorTokenDict.AddOrUpdate(token.SessionId, token, (a, b) => token);

            fiberRw.UserToken = token;
            await token.SendSessionId();
            return await token.RunIt();
        }

        protected async Task<bool> ResetToken(IFiberRw<AsyncToken> fiberRw, AsyncToken token)
        {
            if (token.IsConnect)
            {
                token.DisconnectIt();               
                return false;
            }
            token.Reset(fiberRw);
            fiberRw.UserToken = token;           
            return await token.RunIt();
        }

        protected virtual void DisconnectHandler(string message, ISockAsyncEventAsServer socketAsync, int erorr)
        {
            try
            {
                if (socketAsync.UserToken is AsyncToken token)
                {
                    var time = TimeHelper.GetTime();
                    DisconnectRemoveList.Enqueue(new TimeKey(token.SessionId, time));
                    token.Disconnect();
                }

            }catch( Exception er)
            {
                Log.Error(er);
            }
        }

    }
}
