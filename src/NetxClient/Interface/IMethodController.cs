using Netx.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Netx
{
    public interface IMethodController
    {
        INetxSClient current { get; set; }

        T Get<T>();
    }

    public class MethodControllerBase : IMethodController
    {
        public INetxSClient current { get; set; }

        public T Get<T>() => current.Get<T>();


    }
}
