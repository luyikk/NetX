using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Netx.Async
{
    public class AsyncResultAwaiter<T> : ICriticalNotifyCompletion, INotifyCompletion
    {


        private Action Continuation;

        private  T result;
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


        private bool iscompleted = false;

        public bool IsCompleted { get { return iscompleted; } }

        public void OnCompleted(Action continuation)
        {
            this.Continuation = continuation;
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            this.Continuation = continuation;
        }

        public AsyncResultAwaiter<T> GetAwaiter() => this;


        public T GetResult()
        {
            return result;
        }
    }
}
