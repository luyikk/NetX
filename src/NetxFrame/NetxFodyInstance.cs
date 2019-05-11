using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Netx
{
    public abstract class NetxFodyInstance:NetxAsync
    {    

        protected Dictionary<Type, Type> FodyType { get; set; } = new Dictionary<Type, Type>();


        public virtual T Get<T>()
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
