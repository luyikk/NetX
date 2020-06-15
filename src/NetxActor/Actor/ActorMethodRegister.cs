using System;
using System.Reflection;

namespace Netx.Actor
{
    public class ActorMethodRegister : MethodRegister
    {
        public OpenAccess Access { get; }

        public ActorMethodRegister(Type instenceType, MethodInfo method, OpenAccess access) : base(instenceType, method)
        {
            Access = access;
        }
    }
}
