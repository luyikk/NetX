using System;
using System.Collections.Generic;
using System.Text;

using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Netx;

namespace Netx.Client
{
    public class InstanceRegister:MethodRegister
    {
        public object Instance { get;  }     

        public InstanceRegister(object instance,Type instanceType, MethodInfo method):
            base(instanceType, method)
        {
            Instance = instance;
        }    

    }
}
