using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Netx.Actor
{
    public class ActorResultAwaiter<T> : ICriticalNotifyCompletion, INotifyCompletion
    {

        private Action Continuation;

        private T result;
        public void Completed(T res)
        {
            result = res;
            iscompleted = true;
            Continuation?.Invoke();
        }

        public void Reset()
        {
            iscompleted = false;
            Continuation = null;
        }


        private bool iscompleted;

        public bool IsCompleted { get { return iscompleted; } }

        public void OnCompleted(Action continuation)
        {
            this.Continuation = continuation;
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            this.Continuation = continuation;
        }

        public ActorResultAwaiter<T> GetAwaiter() => this;


        public T GetResult()
        {
            return result;
        }
    }
}
