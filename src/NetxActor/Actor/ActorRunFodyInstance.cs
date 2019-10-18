using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Netx.Loggine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Netx.Actor
{
    public abstract class ActorRunFodyInstance : ActorRunBase
    {

        public ActorRunFodyInstance(IServiceProvider container)
            : base(container)
        {

        }

        protected ConcurrentDictionary<Type, ObjectMethodExecutor> FodyType { get; set; } = new ConcurrentDictionary<Type, ObjectMethodExecutor>();

        public virtual T Get<T>()
        {

            var interfaceType = typeof(T);
            if (!FodyType.ContainsKey(interfaceType))
            {
                var assembly = interfaceType.Assembly;
                var implementationType = assembly.GetType(interfaceType.FullName + "_Builder_Netx_Implementation");
                if (implementationType == null)
                    throw new NetxException($"not find with {interfaceType.FullName} the Implementation", ErrorType.FodyInstallErr);


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
    }
}
