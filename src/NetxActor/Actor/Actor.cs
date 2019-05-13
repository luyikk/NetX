using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Loggine;

namespace Netx.Actor
{
    public class Actor<R> : IActor<R> where R:class
    {
        public const int Idle = 0;
        public const int Open = 1;
        public const int Disposed = 2;

        public const int SleepCmd = -99998983;


        public ActorScheduler ActorScheduler { get; }

        public IServiceProvider Container { get; }

        public ActorController @ActorController { get; }

        public Dictionary<int, ActorMethodRegister> CmdDict { get; }

        private readonly Lazy<ConcurrentQueue<ActorMessage<R>>> actorRunQueue;

        public ConcurrentQueue<ActorMessage<R>> ActorRunQueue { get => actorRunQueue.Value; }

        public ILog Log { get; }

        public int status = Idle;

        public IActorGet ActorGet { get; }

        public int Status => status;

        public int QueueCount => ActorRunQueue.Count;

        public ActorOptionAttribute Option { get; }

        internal event EventHandler<ActorMessage> EventSourcing;

        public bool IsSleep { get; private set; } = true;

        private long lastRuntime = 0;

        /// <summary>
        /// 最后运行时间
        /// </summary>
        public long LastRunTime { get => lastRuntime; }

        /// <summary>
        /// 是否需要休眠
        /// </summary>
        public bool IsNeedSleep
        {
            get
            {
                if (IsSleep)
                    return false;

                return ((TimeHelper.GetTime() - lastRuntime) / 10000) > Option.Ideltime;

            }
        }


        public Actor(IServiceProvider container, IActorGet actorGet, ActorScheduler actorScheduler, ActorController instance)
        {
            this.ActorScheduler = actorScheduler;
         
            this.ActorGet = actorGet;
            this.ActorController = instance;

            var options= instance.GetType().GetCustomAttributes(typeof(ActorOptionAttribute), false);

            if (options != null&&options.Length>0)
            {
                foreach (var attr in options)
                    if (attr is ActorOptionAttribute option)
                        Option = option;

            }
            else
                Option = new ActorOptionAttribute();
            
           
            
            ActorController.ActorGet = ActorGet;
            ActorController.Status = this;
            this.Container = container;
           
            actorRunQueue = new Lazy<ConcurrentQueue<ActorMessage<R>>>();
            Log = new DefaultLog(container.GetRequiredService<ILoggerFactory>().CreateLogger($"Actor-{instance.GetType().Name}"));
            this.CmdDict = LoadRegister(instance.GetType());
            
        }


        private Dictionary<int, ActorMethodRegister> LoadRegister(Type instanceType)
        {
            Dictionary<int, ActorMethodRegister> registerdict = new Dictionary<int, ActorMethodRegister>();

            var methods = instanceType.GetMethods();
            foreach (var method in methods)
                if (method.IsPublic)
                {
                    var attrs = method.GetCustomAttributes(true);



                    List<TAG> taglist = new List<TAG>();
                    OpenAccess openAccess = OpenAccess.Public;
                    foreach (var attr in attrs)
                    {
                        if (attr is TAG attrcmdtype)
                            taglist.Add(attrcmdtype);
                        else if (attr is OpenAttribute access)
                            openAccess = access.Access;
                    }

                    if (taglist.Count > 0)
                    {

                        if (TypeHelper.IsTypeOfBaseTypeIs(method.ReturnType, typeof(Task)) || method.ReturnType == typeof(void) || method.ReturnType == null)
                        {
                            foreach (var tag in taglist)
                            {
                                var sr = new ActorMethodRegister(instanceType, method, openAccess);

                                if (!registerdict.ContainsKey(tag.CmdTag))
                                    registerdict.Add(tag.CmdTag, sr);
                                else
                                {
                                    Log.Error($"Register actor service {method.Name},cmd:{tag.CmdTag} repeat");
                                    registerdict[tag.CmdTag] = sr;
                                }
                            }
                        }
                        else
                            Log.Error($"Register Actor Service Return Type Err:{method.Name},Use void, Task or Task<T>");
                    }
                }

            return registerdict;
        }




        public void Action(long id, int cmd, OpenAccess access, params object[] args)
        {

            if (status == Disposed)
                throw new ObjectDisposedException("this actor is dispose");

            if (Option?.MaxQueueCount > 0)
                if (ActorRunQueue.Count > Option.MaxQueueCount)
                    throw new NetxException($"this actor queue count >{Option.MaxQueueCount}", ErrorType.ActorQueueMaxErr);

            var sa = new ActorMessage<R>(id, cmd, access,args);
            ActorRunQueue.Enqueue(sa);
            try
            {
                Runing().Wait();
            }
            catch (Exception er)
            {                
                Log.Error(er);
            }
        }

        public async ValueTask AsyncAction(long id, int cmd, OpenAccess access, params object[] args)
        {
            if (status == Disposed)
                throw new ObjectDisposedException("this actor is dispose");

            if (Option?.MaxQueueCount > 0)
                if (ActorRunQueue.Count > Option.MaxQueueCount)
                    throw new NetxException($"this actor queue count >{Option.MaxQueueCount}", ErrorType.ActorQueueMaxErr);

            var sa = new ActorMessage<R>(id, cmd, access, args);
            var task = GetResult(sa);
            ActorRunQueue.Enqueue(sa);
            await Runing();

            if (sa.Awaiter.IsCompleted)
                return;
            else
                await task;
        }

        public async ValueTask<R> AsyncFunc(long id, int cmd, OpenAccess access, params object[] args)
        {
          
            if (status == Disposed)
                throw new ObjectDisposedException("this actor is dispose");

            if(Option?.MaxQueueCount>0)
                if(ActorRunQueue.Count>Option.MaxQueueCount)
                    throw new NetxException($"this actor queue count >{Option.MaxQueueCount}",ErrorType.ActorQueueMaxErr);

            var sa = new ActorMessage<R>(id, cmd, access, args);
            var task = GetResult(sa);
            ActorRunQueue.Enqueue(sa);

            await Runing();

            if (sa.Awaiter.IsCompleted)
                return sa.Awaiter.GetResult();
            else
                return await task;          

        }


        private async Task<R> GetResult(ActorMessage<R> actorItem)
        {
            return await actorItem.Awaiter;
        }




        private Task Runing()
        {          
            if (Interlocked.Exchange(ref status, Open) == Idle)
            {
                 async Task RunNext()
                 {
                    try
                    {
                       

                        while (ActorRunQueue.TryDequeue(out ActorMessage<R> msg))
                        {

                            var res = await Call_runing(msg);

                            msg.Awaiter.Completed(res);

                            lastRuntime = TimeHelper.GetTime();

                            if (EventSourcing != null)
                            {
                                msg.CompleteTime= lastRuntime;
                                EventSourcing(ActorController, msg);
                            }

                             

                            if (status == Disposed)
                                break;
                        }
                    }
                    finally
                    {
                        Interlocked.CompareExchange(ref status, Idle, Open);
                    }
                };

               return  ActorScheduler.Scheduler(RunNext);

            }

            return Task.CompletedTask;
        }

        private async Task<R> Call_runing(ActorMessage<R> result)
        {
            var cmd = result.Cmd;
            var args = result.Args;


            #region Awaken and sleep

            if (cmd == SleepCmd)
            {
                await ActorController.Sleeping();
                IsSleep = true;
                return default;
            }
            else if (IsSleep)
            {
                await ActorController.Awakening();
                IsSleep = false;
            }




            #endregion


            if (CmdDict.ContainsKey(cmd))
            {
                var service = CmdDict[cmd];

                if (result.Access>= service.Access)
                {

                    if (service.ArgsLen == args.Length)
                    {
                        ActorController.OrderTime = result.CompleteTime;

                        switch (service.ReturnMode)
                        {
                            case ReturnTypeMode.Null:
                                {
                                    ActorController.Runs__Make(cmd, args);
                                    return default;
                                }
                            case ReturnTypeMode.Task:
                                {
                                    await (dynamic)ActorController.Runs__Make(cmd, args);
                                    return default;
                                }
                            case ReturnTypeMode.TaskValue:
                                {
                                    return await (dynamic)ActorController.Runs__Make(cmd, args);
                                }
                            default:
                                {
                                    throw new NetxException("not find the return mode", ErrorType.ReturnModeErr);
                                }

                        }
                    }
                    else
                    {
                        return (R)GetErrorResult($"actor cmd:{cmd} args count error", result.Id);
                    }
                }
                else
                {
                    throw new NetxException($"actor cmd:{cmd} permission denied", ErrorType.PermissionDenied);                   
                }
            }
            else
            {
                return (R)GetErrorResult($"not find actor cmd:{cmd}", result.Id);              
            }           
        }

        public object GetErrorResult(string msg, long id)
        {
            Result err = new Result()
            {
                ErrorMsg = msg,
                Id = id,
                ErrorId = (int)ErrorType.ActorErr
            };

            return err;
        }


        public void Dispose()
        {
            if (Interlocked.Exchange(ref status, Disposed) != Disposed)
            {
                CmdDict.Clear();

                while (ActorRunQueue.Count>0)                
                    ActorRunQueue.TryDequeue(out _);                
            }           
        }
       
    }
}
