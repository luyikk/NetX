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


        public IServiceProvider Container { get; }

        public ActorController @ActorController { get; }

        public Dictionary<int,MethodRegister> CmdDict { get; }

        private readonly Lazy<ConcurrentQueue<SingleActorItem<R>>> actorRunQueue;

        public ConcurrentQueue<SingleActorItem<R>> ActorRunQueue { get => actorRunQueue.Value; }

        public ILog Log { get; }

        public int status = Idle;

        public IActorGet ActorGet { get; }

        public int Status => status;

        public int QueueCount => ActorRunQueue.Count;



        public Actor(IServiceProvider container, IActorGet actorGet, ActorController instance)
        {           
            this.ActorGet = actorGet;
            this.ActorController = instance;
            ActorController.ActorGet = ActorGet;
            ActorController.Status = this;
            this.Container = container;
           
            actorRunQueue = new Lazy<ConcurrentQueue<SingleActorItem<R>>>();
            this.CmdDict = LoadRegister(instance.GetType());
            Log = new DefaultLog(container.GetRequiredService<ILoggerFactory>().CreateLogger($"Actor-{instance.GetType().Name}"));
        }


        private Dictionary<int, MethodRegister> LoadRegister(Type instanceType)
        {
            Dictionary<int, MethodRegister> registerdict = new Dictionary<int, MethodRegister>();

            var methods = instanceType.GetMethods();
            foreach (var method in methods)
                if (method.IsPublic)
                    foreach (var attr in method.GetCustomAttributes(typeof(TAG), true))
                        if (attr is TAG attrcmdtype)
                        {
                            if (TypeHelper.IsTypeOfBaseTypeIs(method.ReturnType, typeof(Task)) || method.ReturnType == typeof(void) || method.ReturnType == null)
                            {
                                var sr = new MethodRegister(instanceType, method);

                                if (!registerdict.ContainsKey(attrcmdtype.CmdTag))
                                    registerdict.Add(attrcmdtype.CmdTag, sr);
                                else
                                {
                                    Log.Error($"Register actor service {method.Name},cmd:{attrcmdtype.CmdTag} repeat");
                                    registerdict[attrcmdtype.CmdTag] = sr;
                                }
                            }
                            else
                                Log.Error($"Register Actor Service Return Type Err:{method.Name},Use void, Task or Task<T>");
                        }

            return registerdict;
        }




        public void Action(long id,int cmd, params object[] args)
        {
            if (status == Disposed)
                throw new ObjectDisposedException("this Actor is Close");

            var sa = new SingleActorItem<R>(id,cmd,args);
            ActorRunQueue.Enqueue(sa);
            Runing();
        }

        public async ValueTask AsyncAction(long id, int cmd, params object[] args)
        {
            if (status == Disposed)
                throw new ObjectDisposedException("this Actor is Close");

            var sa = new SingleActorItem<R>(id, cmd, args);
            var task = GetResult(sa);
            ActorRunQueue.Enqueue(sa);          
            Runing();

            if (sa.Awaiter.IsCompleted)
                return;
            else
                await task;
        }

        public async ValueTask<R> AsyncFunc(long id, int cmd, params object[] args)
        {
          
            if (status == Disposed)
                throw new ObjectDisposedException("this Actor is Close");

            var sa = new SingleActorItem<R>(id, cmd, args);
            var task = GetResult(sa);
            ActorRunQueue.Enqueue(sa);

            Runing();

            if (sa.Awaiter.IsCompleted)
                return sa.Awaiter.GetResult();
            else
                return await task;

        }


        private async Task<R> GetResult(SingleActorItem<R> actorItem)
        {
            return await actorItem.Awaiter;
        }




        private void Runing()
        {
            if (status == Disposed)
                throw new ObjectDisposedException("this Actor is Close");

            if (Interlocked.Exchange(ref status, Open) == Idle)
            {
                Task.Factory.StartNew(async () =>
                {

                    while (ActorRunQueue.TryDequeue(out SingleActorItem<R> reuslt))
                    {
                        var res = await Call_runing(reuslt);
                        reuslt.Awaiter.Completed(res);
                        if (status == Disposed)
                            break;
                    }


                    Interlocked.CompareExchange(ref status, Idle, Open);

                });
            }

        }

        private async Task<R> Call_runing(SingleActorItem<R> result)
        {
            var cmd = result.Cmd;
            var args = result.Args;

            if (CmdDict.ContainsKey(cmd))
            {
                var service = CmdDict[cmd];

                if (service.ArgsLen == args.Length)
                {
                    ActorController.OrderTime = result.PushTime;

                    switch (service.ReturnMode)
                    {
                        case ReturnTypeMode.Null:
                            {
                                service.Method.Invoke(ActorController, args);
                                return null;
                            }                           
                        case ReturnTypeMode.Task:
                            {
                                await (Task)service.Method.Invoke(ActorController, args);
                                return null;
                            }                           
                        case ReturnTypeMode.TaskValue:
                            {
                                return await (dynamic)service.Method.Invoke(ActorController, args);
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
