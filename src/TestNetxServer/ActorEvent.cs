using Netx.Actor;
using Netx.Loggine;
using Netx.Service;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TestNetxServer
{
    public class ActorEvent1 : ActorEventBase
    {
        private ILog Log { get; }

        public ActorEvent1(IServiceProvider container)
        {
            Log = new DefaultLog(container.GetRequiredService<ILogger<ActorEvent1>>());
        }

        public override void ActorCompletedEvent(object actorController, ActorMessage actorMessage)
        {
            Log.Trace($"PushTime:{TimeHelper.GetTime(actorMessage.PushTime)}  Cmd:{actorMessage.Cmd} Completed");
        }
    }

    public class ActorEvent2 : ActorEventBase
    {
        private ILog Log { get; }

        public ActorEvent2(IServiceProvider container)
        {
            Log = new DefaultLog(container.GetRequiredService<ILogger<ActorEvent2>>());
        }

        public override void ActorCompletedEvent(object actorController, ActorMessage actorMessage)
        {
          switch(actorController)
            {
                case TestActorController testActorController:
                    {
                        Log.Trace($"I was called {testActorController.UseCount} times. ");
                    }
                    break;
            }
           
        }
    }
}
