using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ZYSocket.FiberStream;

namespace Netx.Service
{
    public abstract class ServiceToken:ServiceInstall
    {

        private readonly Lazy<ServiceTokenFactory> LazyServiceTokenFactory;

        protected ServiceTokenFactory TokenFactory { get => LazyServiceTokenFactory.Value; }

        protected ConcurrentDictionary<long, AsyncToken> ActorTokenDict { get; }

        public ServiceToken(IServiceProvider container) :base(container)
        {
            ActorTokenDict = new ConcurrentDictionary<long, AsyncToken>();
            LazyServiceTokenFactory = new Lazy<ServiceTokenFactory>(() => new ServiceTokenFactory(container),true);
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

        protected async Task<bool> ResetToken(IFiberRw<AsyncToken> fiberRw, AsyncToken actorToken)
        {
            actorToken.Reset(fiberRw);
            fiberRw.UserToken = actorToken;           
            return await actorToken.RunIt();
        }
    }
}
