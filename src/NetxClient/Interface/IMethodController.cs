using Netx.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Netx
{
    public interface IMethodController
    {
        INetxSClientBase? Current { get; set; }       
    }

    public class MethodControllerBase : IMethodController
    {
        public INetxSClientBase? Current { get; set; }      
    }
}
