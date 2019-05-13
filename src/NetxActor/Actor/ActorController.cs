using System.Threading.Tasks;

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
       
        /// <summary>
        /// 唤醒
        /// </summary>
        public virtual Task Awakening()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 沉睡
        /// </summary>
        public virtual Task Sleeping()
        {
            return Task.CompletedTask;
        }
    }
}
