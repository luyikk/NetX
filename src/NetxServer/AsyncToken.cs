using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netx.Actor;
using Netx.Interface;
using Netx.Loggine;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZYSocket.FiberStream;

namespace Netx.Service
{
    public class AsyncToken : AsyncBuffer
    {

        private bool is_dispose = false;

        public ConcurrentDictionary<int, MethodRegister> AsyncServicesRegisterDict { get; }

        private readonly Lazy<Dictionary<Type, AsyncController>> asyncControllerInstanceDict;

        public Dictionary<Type, AsyncController> AsyncControllerInstanceDict { get => asyncControllerInstanceDict.Value; }     

        public ActorRun @ActorRun { get; }       

        public AsyncToken(IServiceProvider container, IFiberRw<AsyncToken> fiberRw, ConcurrentDictionary<int, MethodRegister> asyncServicesRegisterDict, long sessionId)
            : base(container, fiberRw, sessionId)
        {
            this.RequestOutTime= container.GetRequiredService<IOptions<ServiceOption>>().Value.ClearRequestTime;
            ActorRun = container.GetRequiredService<ActorRun>();
            AsyncServicesRegisterDict = asyncServicesRegisterDict;
            asyncControllerInstanceDict = new Lazy<Dictionary<Type, AsyncController>>();
           
        }


        internal void Close()
        {
            if (!is_dispose)
            {
                is_dispose = true;

                foreach (var item in AsyncControllerInstanceDict.Values)
                {
                    try { item.Closed(); }
                    catch (Exception er)
                    {
                        Log.Error(er);
                    }

                }

                AsyncControllerInstanceDict.Clear();



                if (this.IsConnect)
                {
                    this.IsConnect = false;
                    if (FiberRw != null)
                    {
                        this.FiberRw.UserToken = null;
                        this.FiberRw.Async?.Disconnect();                       
                    }
                    this.IWrite = null;

                }




            }
        }

        public void DisconnectIt()
        {         
            FiberRw?.Async?.Disconnect();
        }


        public override T Get<T>()
        {
            if (is_dispose)
                throw new ObjectDisposedException("AsyncToken");

            return base.Get<T>();
        }


        public T Actor<T>()
        {          
            return ActorRun.Get<T>();
        }       



        protected override bool ConnectIt()
        {
            return false;
        }

        internal async Task<bool> RunIt()
        {
            if (is_dispose)
                throw new ObjectDisposedException("AsyncToken");

            while (isConnect)
            {
                if (!await DataOnByLine(FiberRw!))
                    break;               
            }

            isConnect = false;
            return false;
        }

        internal void Reset(IFiberRw<AsyncToken> fiberRw)
        {
            FiberRw = fiberRw;
            isConnect = true;          
        }


        internal void Disconnect()
        {
            isConnect = false;

            this.IWrite = null;
            this.FiberRw = null;
            foreach (var controller in AsyncControllerInstanceDict.Values)            
               controller.Disconnect(); 
        }

       

        protected async Task<bool> DataOnByLine(IFiberRw<AsyncToken> fiberRw)
        {
          
            var cmd = await fiberRw.ReadInt32();        

            switch (cmd)
            {
                case 2400:
                    {
                        var type = await fiberRw.ReadByte();
                        switch (type)
                        {
                            case 0: //RUN CALL NOT RES
                                {
                                    return await DataOnByRead(fiberRw, 0);
                                }
                            case 1: //RUN CALL HAVE RES
                                {
                                    return await DataOnByRead(fiberRw, 1);
                                }
                            case 2: // RUN CALL RETURN 
                                {
                                    return await DataOnByRead(fiberRw, 2);
                                }
                        }

                        return true;
                    }               
                case 2500: //set result
                    {
                        await ReadResultAsync(fiberRw);
                    }
                    break;

            }

            return true;
        }



        protected async Task<bool> DataOnByRead(IFiberRw<AsyncToken> fiberRw, int runtype)
        {
            var cmd = await fiberRw.ReadInt32();
            var id = await fiberRw.ReadInt64();
            if (AsyncServicesRegisterDict.TryGetValue(cmd, out MethodRegister? service))
            {
                var argslen = await fiberRw.ReadInt32();
                if (argslen == service.ArgsLen)
                {
                    object[] args = new object[argslen];
                    List<IMemoryOwner<byte>> mem_disposetable = new List<IMemoryOwner<byte>>();

                    for (int i = 0; i < argslen; i++)
                    {
                        var (arg, owner) = await base.ReadDataAsync(fiberRw, service.ArgsType[i]);
                        args[i] = arg;
                        if (owner != null)
                            mem_disposetable.Add(owner);
                    }

                    RunCall(service, cmd, id, runtype, mem_disposetable, args);
                    return true;

                }
                else
                {
                    Log.WarnFormat($"{fiberRw.Async?.AcceptSocket?.RemoteEndPoint} call async service:{cmd} Args Error: len {argslen}->{ service.ArgsType.Length} \r\n to {service}");
                    await SendError(id, $"call async service:{cmd} Args Error: len {argslen}->{service.ArgsType.Length}\r\n to {service}", ErrorType.ArgLenErr);
                    return false;
                }
            }
            else
            {
                
                service = ActorRun.GetCmdService(cmd);
                if (service != null)
                {
                    var argslen = await fiberRw.ReadInt32();
                    if (argslen == service.ArgsLen)
                    {
                        object[] args = new object[argslen];

                        List<IMemoryOwner<byte>> mem_disposetable = new List<IMemoryOwner<byte>>();
                        for (int i = 0; i < argslen; i++)
                        {
                            var (arg, owner) = await base.ReadDataAsync(fiberRw, service.ArgsType[i]);
                            args[i] = arg;
                            if (owner != null)
                                mem_disposetable.Add(owner);
                        }


                        RunActor(cmd, id, runtype, mem_disposetable, args);
                        return true;

                    }
                    else
                    {
                        Log.WarnFormat($"{ fiberRw.Async?.AcceptSocket?.RemoteEndPoint} call actor service:{cmd} Args Error: len {argslen}->{service.ArgsType.Length} \r\n to {service}");
                        await SendError(id, $"call actor service:{cmd} Args Error: len {argslen}->{service.ArgsType.Length}\r\n to {service}", ErrorType.ArgLenErr);
                        return false;
                    }

                }
                else
                {
                    Log.WarnFormat($"{fiberRw.Async?.AcceptSocket?.RemoteEndPoint} call service:{cmd} not find cmd ");
                    await SendError(id, $"call service:{cmd} not find the cmd,please check it", ErrorType.NotCmd);
                    return false;
                }
            }


        }


        protected virtual async void RunActor(int cmd, long id, int runtype, List<IMemoryOwner<byte>> memoryOwners, params object[] args)
        {
            try
            {

                switch (runtype)
                {
                    case 0:
                        {
                            ActorRun.SyncAction(id, cmd, OpenAccess.Public, args);
                            Dispose_table(memoryOwners);
                        }
                        break;
                    case 1:
                        {
                            await (ValueTask)ActorRun.AsyncAction(id, cmd, OpenAccess.Public, args);
                            Dispose_table(memoryOwners);
                            await SendResult(id);
                        }
                        break;
                    case 2:
                        {

                            var ret_value = await ActorRun.CallFunc<object>(id, cmd, OpenAccess.Public, args);
                            Dispose_table(memoryOwners);
                            switch (ret_value)
                            {
                                case Result result:
                                    {
                                        result.Id = id;
                                        await SendResult(result);
                                    }
                                    break;
                                default:
                                    {
                                        await SendResult(id, ret_value);
                                    }
                                    break;
                            }
                        }
                        break;

                }

              
            }
            catch (System.Net.Sockets.SocketException)
            {

            }
            catch (NetxException er)
            {
                if(er.ErrorType!=ErrorType.ActorQueueMaxErr)
                    Log.Error(er);
                await SendError(id, $"Actor Server Err:{er.Message}", ErrorType.CallErr);
            }
            catch (Exception er)
            {
                Log.Error(er);
                await SendError(id, $"Actor Server Err:{er.Message}", ErrorType.CallErr);
            }
        }

    
       

        protected virtual async void RunCall(MethodRegister service, int cmd, long id, int runtype, List<IMemoryOwner<byte>> memoryOwners,  params object[] args)
        {
            try
            {
                var controller = await GetInstance(id, cmd, service.InstanceType);

           
                if (controller != null)
                    await RunControllerService(service, controller,cmd, id, runtype, memoryOwners, args);

            }
            catch(System.Net.Sockets.SocketException)
            {

            }
            catch (Exception er)
            {
                Log.Error(er);
                await SendError(id, $"Async Server Err:{er.Message}", ErrorType.CallErr);
            }
        }

        protected virtual async Task RunControllerService(MethodRegister service, AsyncController controller,int cmd, long id, int runType, List<IMemoryOwner<byte>> memoryOwners, params object[] args)
        {
          
            switch (service.ReturnMode)
            {

                case ReturnTypeMode.Null:
                    if (runType == 0)
                    {
                        controller.Runs__Make(cmd, args);
                        Dispose_table(memoryOwners);
                        return;
                    }
                    break;
                case ReturnTypeMode.Task:
                    if (runType == 1)
                    {
                        await (dynamic) controller.Runs__Make(cmd, args);
                        Dispose_table(memoryOwners);
                        await SendResult(id);
                        return;
                    }
                    break;
                case ReturnTypeMode.TaskValue:
                    if (runType == 2)
                    {
                        var ret_value = await (dynamic)controller.Runs__Make(cmd, args);
                        Dispose_table(memoryOwners);
                        switch (ret_value)
                        {
                            case Result result:
                                {
                                    result.Id = id;
                                    await SendResult(result);
                                    return;
                                }
                            default:
                                {
                                   
                                    await SendResult(id, ret_value);                                   
                                    return;
                                }

                        }
                    }
                    break;



            }

            await SendNotRunType(service, id, runType);
        }


        protected virtual async ValueTask<AsyncController?> GetInstance(long id, int cmd, Type instanceType)
        {
            if (!AsyncControllerInstanceDict.ContainsKey(instanceType))
            {
                var constructors = instanceType.GetConstructors();
                if (constructors.Length == 1)
                {
                    var constructor = constructors[0];
                    if (!constructor.IsGenericMethod)
                    {
                        if (constructor.IsPublic)
                        {
                            try
                            {
                                var instance = (AsyncController)Container.GetRequiredService(instanceType);
                                instance.Async = this;
                                AsyncControllerInstanceDict.Add(instanceType, instance);
                                return instance;
                            }
                            catch (InvalidOperationException er)
                            {
                                Log.WarnFormat($"{FiberRw?.Async?.AcceptSocket?.RemoteEndPoint} call async service:{cmd}  not create instance from {instanceType.FullName} Error:\r\n{er}");
                                await SendError(id, $"call async service:{cmd} not create instance from {instanceType.FullName} Error:{er.ToString()}", ErrorType.CreateInstanceErr);
                                return null;
                            }
                        }
                        else
                        {
                            Log.WarnFormat($"{FiberRw?.Async?.AcceptSocket?.RemoteEndPoint} call async service:{cmd}  Constructor  not is public");
                            await SendError(id, $"call async service:{cmd} Constructor not is public", ErrorType.ConstructorsErr);
                            return null;
                        }
                    }
                    else
                    {
                        Log.WarnFormat($"{FiberRw?.Async?.AcceptSocket?.RemoteEndPoint} call async service:{cmd}  Constructor  not is Generic");
                        await SendError(id, $"call async service:{cmd} Constructor not is Generic", ErrorType.ConstructorsErr);
                        return null;
                    }
                }
                else
                {
                    Log.WarnFormat($"{FiberRw?.Async?.AcceptSocket?.RemoteEndPoint} call async service:{cmd} Constructor count error,need use 1 Constructor ");
                    await SendError(id, $"call async service:{cmd} Constructor count error,need use 1 Constructor", ErrorType.ConstructorsErr);
                    return null;
                }
            }
            else
                return AsyncControllerInstanceDict[instanceType];

        }

     
    }
}
