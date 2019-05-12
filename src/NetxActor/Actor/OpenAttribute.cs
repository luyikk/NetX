using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Actor
{
    public enum OpenAccess
    {
        Public = 0,
        Internal = 2,
        Private = 3,
       
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class OpenAttribute : Attribute
    {      
        public OpenAccess Access { get; }

        public OpenAttribute(OpenAccess access)
        {
            Access = access;
        }

    }
}
