using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Loggine;
using System;
using System.Collections.Generic;

namespace Netx.Actor
{
    public abstract class ActorRunFodyInstance : ActorRunBase
    {

        public ActorRunFodyInstance(IServiceProvider container)
            : base(container)
        {

        }

        protected Dictionary<Type, Type> FodyType { get; set; } = new Dictionary<Type, Type>();

        public T Get<T>()
        {
            var interfaceType = typeof(T);
            if (!FodyType.ContainsKey(interfaceType))
            {
                var assembly = interfaceType.Assembly;
                var implementationType = assembly.GetType(interfaceType.FullName + "_Builder_Netx_Implementation");
                if (implementationType == null)
                    throw new NetxException($"not find with {interfaceType.FullName} the Implementation", ErrorType.FodyInstallErr);
                FodyType.Add(interfaceType, implementationType);
                return (T)Activator.CreateInstance(implementationType, this);

            }
            else
            {
                return (T)Activator.CreateInstance(FodyType[interfaceType], this);
            }
        }
    }
}
