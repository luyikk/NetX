using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Actor
{
    public interface IActorGet
    {
        T Get<T>();
    }
}
