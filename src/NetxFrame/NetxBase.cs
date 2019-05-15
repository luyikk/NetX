using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Netx.Async;
using Netx.Interface;
using Netx.Loggine;

namespace Netx
{
    public abstract class NetxBase: INetxBuildInterface
    {

        protected IIds IdsManager { get; set; }
     
        /// <summary>
        /// 日记输出
        /// </summary>
        public ILog Log { get; protected set; }

        /// <summary>
        /// 用于存放异步调用时,结果反馈的回调
        /// </summary>
        private readonly Lazy<Dictionary<long, AsyncResultAwaiter<Result>>> asyncResultDict = new Lazy<Dictionary<long, AsyncResultAwaiter<Result>>>(() =>
          new Dictionary<long,AsyncResultAwaiter<Result>>(50) //这里不使用 Concurrent 因为我希望他是单线程访问的
        , true);

        /// <summary>
        /// 用于存放异步调用时,结果反馈的回调
        /// </summary>
        protected Dictionary<long,AsyncResultAwaiter<Result>> AsyncResultDict { get => asyncResultDict.Value; }

        /// <summary>
        /// 用来超时处理
        /// </summary>
        private readonly Lazy<ConcurrentQueue<RequestKeyTime>> requestOutTimeQueue = new Lazy<ConcurrentQueue<RequestKeyTime>>(true);

        protected ConcurrentQueue<RequestKeyTime> RequestOutTimeQueue { get => requestOutTimeQueue.Value; }

        /// <summary>
        /// 调用超时时间
        /// </summary>
        public long RequestOutTime { get; protected set; }= 10000;

        /// <summary>
        /// 运行，不等待返回结果,不会阻止当前线程
        /// </summary>
        /// <param name="cmdTag">命令</param>
        /// <param name="args">参数</param>
        public void Action(int cmdTag, params object[] args) => SendAction(cmdTag, args);


        /// <summary>
        /// 异步运行,调用此函数会Return当前线程,直到完成调用后使用通知线程运行接下去的上下文
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public Task AsyncAction(int cmdTag, params object[] args) => SendAsyncAction(cmdTag, IdsManager.MakeId, args);


        /// <summary>
        /// 运行函数异步方式,调用此函数会Return当前线程,直到结果返回后使用返回结果线程运行接下去的上下文
        /// </summary>
        /// <param name="cmdTag">命令</param>
        /// <param name="args">参数</param>
        /// <returns>返回结果</returns>
        public Task<IResult> AsyncFunc(int cmdTag, params object[] args) => AsyncFuncSend(cmdTag, IdsManager.MakeId, args);


        /// <summary>
        /// 运行函数异步方式,调用此函数会Return当前线程,直到结果返回后使用返回结果线程运行接下去的上下文,泛型方式
        /// </summary>
        /// <typeparam name="S">泛型类型</typeparam>
        /// <param name="cmdtag">命令</param>
        /// <param name="args">参数</param>
        /// <returns>返回结果</returns>
        public async Task<T> AsyncFunc<T>(int cmdtag, params object[] args)
        {
            var res = await AsyncFunc(cmdtag, args);
            return res.As<T>();
        }



        /// <summary>
        /// 运行调用,同步等待发送到服务器,不等待运行结束
        /// </summary>
        /// <param name="cmdTag">命令</param>
        /// <param name="args">需要发送参数</param>
        protected abstract void SendAction(int cmdTag, object[] args);

        /// <summary>
        /// 运行调用,同步等待结束
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected abstract Task SendAsyncAction(int cmdTag, long Id, object[] args);
        

        /// <summary>
        /// 运行函数异步方式
        /// </summary>
        /// <param name="cmdTag">命令</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        protected abstract Task<IResult> AsyncFuncSend(int cmdTag,long Id, object[] args);


        public object Func(int cmdTag, Type type, params object[] args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 添加异步回调到表中
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        protected virtual AsyncResultAwaiter<Result> AddAsyncResult(long ids)
        {
            AsyncResultAwaiter<Result> asyncResult = new AsyncResultAwaiter<Result>();
            if (!AsyncResultDict.ContainsKey(ids))
                AsyncResultDict.Add(ids, asyncResult);
            else
            {
                Log.Info($"add async back have id:{ids}");
                AsyncResultDict[ids] = asyncResult;
            }

            if(RequestOutTime>0)
                RequestOutTimeQueue.Enqueue(new RequestKeyTime(ids, TimeHelper.GetTime()));

            return asyncResult;
        }

        protected void Dispose_table(List<IMemoryOwner<byte>> memDisposableList)
        {
            if (memDisposableList.Count > 0)
            {
                foreach (var mem in memDisposableList)
                    mem.Dispose();
                memDisposableList.Clear();
            }
        }




        protected struct RequestKeyTime
        {
            public long Key { get;  }

            public long Time { get;  }

            public RequestKeyTime(long key,long time)
            {
                Key = key;
                Time = time;
            }
        }

    }
}
