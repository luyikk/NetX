using System;

using System.Reflection;

namespace Netx.Client
{
    public class InstanceRegister : MethodRegister
    {
        public object Instance { get; }

        public InstanceRegister(object instance, Type instanceType, MethodInfo method) :
            base(instanceType, method)
        {
            Instance = instance;
        }

    }
}
