﻿using Microsoft.Extensions.Internal;
using Netx.Interface;
using Netx.Loggine;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Netx
{

    public abstract class NetxFodyInstance : NetxAsync
    {
        public NetxFodyInstance(ILog log, IIds idsManager) : base(log, idsManager) { }

        protected Dictionary<Type, ObjectMethodExecutor> FodyType { get; set; } = new Dictionary<Type, ObjectMethodExecutor>();

        public virtual T Get<T>()
        {

            var interfaceType = typeof(T);
            if (!FodyType.ContainsKey(interfaceType))
            {
                var assembly = interfaceType.Assembly;
                var implementationType = assembly.GetType(interfaceType.FullName + "_Builder_Netx_Implementation") ?? throw new NetxException($"not find with {interfaceType.FullName} the Implementation", ErrorType.FodyInstallErr);
                var getImplementation = implementationType.GetMethod("GetImplementation", BindingFlags.Static | BindingFlags.Public);

                var method = ObjectMethodExecutor.Create(getImplementation, null!);
#if NETSTANDARD2_0
                if (!FodyType.ContainsKey(interfaceType))
                    FodyType.Add(interfaceType, method);
#else
                FodyType.TryAdd(interfaceType, method);
#endif

                return (T)method.Execute(null!, new object[] { this });

            }
            else
            {
                return (T)FodyType[interfaceType].Execute(null!, new object[] { this });
            }
        }

    }
}
