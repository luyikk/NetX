using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Actor;
using Netx.Interface;
using ZYSocket.Interface;

namespace Netx.Actor.Builder
{
    public interface IActorBuilder:IDisposable
    {
        IServiceCollection? Container { get; }
        IServiceProvider? Provider { get; }

        IActorBuilder AddActorEvent<T>() where T : ActorEventBase;
        IActorRun Build();
        IActorBuilder ConfigIIds(Func<IServiceProvider, IIds>? func = null);
        IActorBuilder ConfigObjFormat(Func<ISerialization>? func = null);
        IActorBuilder ConfigureActorScheduler(Func<IServiceProvider, ActorScheduler>? func = null);
        IActorBuilder ConfigureLogSet(Action<ILoggingBuilder>? config = null);
        IActorBuilder RegisterService(Assembly assembly);
        IActorBuilder RegisterService(Type controller_instance_type);
        IActorBuilder RegisterService<ActorType>() where ActorType : ActorController;
        IActorBuilder RegisterDescriptors(Action<IServiceCollection> serviceDescriptors);
        IActorBuilder UseActorLambda();
    }
}