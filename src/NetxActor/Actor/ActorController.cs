namespace Netx.Actor
{
    public abstract class ActorController
    {
        public long OrderTime { get; internal set; }
        public long CurrentTime { get => TimeHelper.GetTime(); }
        public long PassTime { get => CurrentTime - OrderTime; }
        public IActorGet ActorGet { get; internal set; }
        public IActorStatus Status { get; internal set; }

        protected T Get<T>() => ActorGet.Get<T>();

        public virtual object Runs__Make(int tag, object[] args) => null;
       
    }
}
