using Netx.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Netx
{
    public interface IMethodController
    {
        INetxSClient Current { get; set; }

        T Get<T>();
    }

    public class MethodControllerBase : IMethodController
    {
        public INetxSClient Current { get; set; }

        public T Get<T>() => Current.Get<T>();


    }
}
