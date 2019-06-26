using Netx.Actor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Actor
{
    public abstract class ActorEventBase
    {
        public abstract void ActorEventCompleted(object actorController, IActorMessage actorMessage);       
    }
}
