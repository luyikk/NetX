namespace Netx.Service
{
    public abstract class AsyncController
    {
        internal AsyncToken? Async { get; set; }

        protected AsyncToken? Current { get => Async; }

        public T? Get<T>() where T : class => Current?.Get<T>();
        public T? Actor<T>() where T : class => Current?.Actor<T>();

        public virtual object Runs__Make(int tag, object[] args) => null!;

        /// <summary>
        /// 断线处理
        /// </summary>
        public virtual void Disconnect()
        {

        }

        /// <summary>
        /// 彻底结束
        /// </summary>
        public virtual void Closed()
        {

        }


    }
}
