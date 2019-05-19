using Netx.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Service
{
    public abstract class ActorEventBase
    {
        public abstract void ActorEventSourcing(object actorController, IActorMessage actorMessage);       
    }
}
