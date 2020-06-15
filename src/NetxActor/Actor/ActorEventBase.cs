namespace Netx.Actor
{
    public abstract class ActorEventBase
    {
        public abstract void ActorEventCompleted(object actorController, IActorMessage actorMessage);
    }
}
