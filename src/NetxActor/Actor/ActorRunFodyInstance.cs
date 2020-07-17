using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Netx.Actor
{
    public abstract class ActorRunFodyInstance : ActorRunBase
    {

        public ActorRunFodyInstance(IServiceProvider container,ILogger logger)
            : base(container,logger)
        {
            lambdaActorRuns = new Lazy<ConcurrentDictionary<string, LambdaActorRun>>();
        }

        protected ConcurrentDictionary<Type, ObjectMethodExecutor> FodyType { get; set; } = new ConcurrentDictionary<Type, ObjectMethodExecutor>();

        private readonly Lazy<ConcurrentDictionary<string, LambdaActorRun>> lambdaActorRuns;
        public ConcurrentDictionary<string, LambdaActorRun> LambdaActorRunCollect => lambdaActorRuns.Value;

        public virtual T Get<T>()
        {

            var interfaceType = typeof(T);
            if (!FodyType.ContainsKey(interfaceType))
            {
                var assembly = interfaceType.Assembly;
                var implementationType = assembly.GetType(interfaceType.FullName + "_Builder_Netx_Implementation");
                if (implementationType == null)
                    throw new NetxException($"not found with {interfaceType.FullName} the Implementation", ErrorType.FodyInstallErr);


                var getImplementation = implementationType.GetMethod("GetImplementation", BindingFlags.Static | BindingFlags.Public);

                var method = ObjectMethodExecutor.Create(getImplementation, null!);
                FodyType.TryAdd(interfaceType, method);

                return (T)method.Execute(null!, new object[] { this });

            }
            else
            {
                return (T)FodyType[interfaceType].Execute(null!, new object[] { this });
            }
        }


        public virtual IActorLambda GetLambda(string key = "default")
        {
            if (LambdaActorRunCollect.TryGetValue(key, out var lambdaRun))
            {
                return lambdaRun.Get<IActorLambda>();
            }
            else 
                throw new NetxException($"not found lambda Actor key:{key}",ErrorType.FodyInstallErr);
        }

    }
}
