namespace Netx.Service
{
    public abstract class AsyncController
    {
        internal AsyncToken? Async { get; set; }

        protected AsyncToken? Current => Async;

        public T? Get<T>() where T : class
        {
            return Current?.Get<T>();
        }

        public T? Actor<T>() where T : class
        {
            return Current?.Actor<T>();
        }

        public virtual object Runs__Make(int tag, object[] args)
        {
            return null!;
        }

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
