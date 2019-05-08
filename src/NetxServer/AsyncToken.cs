using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Netx.Actor;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZYSocket.FiberStream;

namespace Netx.Service
{
    public class AsyncToken : AsyncBase
    {


        public ConcurrentDictionary<int, MethodRegister> AsyncServicesRegisterDict { get; }

        private readonly Lazy<Dictionary<Type, AsyncController>> asyncControllerInstanceDict;

        public Dictionary<Type, AsyncController> AsyncControllerInstanceDict { get => asyncControllerInstanceDict.Value; }     

        public ActorRun @ActorRun { get; }

        public AsyncToken(IServiceProvider container, IFiberRw<AsyncToken> fiberRw, ConcurrentDictionary<int, MethodRegister> asyncServicesRegisterDict, long sessionId)
            : base(container, fiberRw, sessionId)
        {

            ActorRun = container.GetRequiredService<ActorRun>();
            AsyncServicesRegisterDict = asyncServicesRegisterDict;
            asyncControllerInstanceDict = new Lazy<Dictionary<Type, AsyncController>>();
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
          
            while (isConnect)
            {
                if (!await DataOnByLine(FiberRw))
                    break;               
            }

            isConnect = false;
            return false;
        }

        internal void Reset(IFiberRw<AsyncToken> fiberRw)
        {
            FiberRw = fiberRw;
            isConnect = true;
            DisconnectTime = DateTime.MaxValue;
        }


        public void Disconnect()
        {
            DisconnectTime = DateTime.Now;
            isConnect = false;
        }

       

        protected async Task<bool> DataOnByLine(IFiberRw<AsyncToken> fiberRw)
        {
          
            var cmd = await fiberRw.ReadInt32();        

            switch (cmd)
            {
                case 2500:
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
            }

            return true;
        }



        protected async Task<bool> DataOnByRead(IFiberRw<AsyncToken> fiberRw, int runtype)
        {
            var cmd = await fiberRw.ReadInt32();
            if (cmd.HasValue)
            {
                var id = (await fiberRw.ReadInt64()).GetValueOrDefault(-1);
                if (AsyncServicesRegisterDict.TryGetValue(cmd.Value, out MethodRegister service))
                {
                    var argslen = (await fiberRw.ReadInt32()).Value;
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


                        RunCall(service, cmd.Value, id, runtype, mem_disposetable,args);
                        return true;

                    }
                    else
                    {
                        Log.WarnFormat("{3} call async service:{0} Args Error: len {1}->{2} \r\n to {4}", cmd.Value, argslen, service.ArgsType.Length, fiberRw.Async?.AcceptSocket?.RemoteEndPoint, service);
                        await SendError(id, $"call async service:{cmd.Value} Args Error: len {argslen}->{service.ArgsType.Length}\r\n to {service}", ErrorType.ArgLenErr);
                        return false;
                    }
                }
                else
                {
                    var cmdTag = cmd.Value;
                    service = ActorRun.GetCmdService(cmdTag);
                    if (service!=null)
                    {
                        var argslen = (await fiberRw.ReadInt32()).Value;
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


                            RunActor(cmdTag, id, runtype, mem_disposetable, args);
                            return true;

                        }
                        else
                        {
                            Log.WarnFormat("{3} call actor service:{0} Args Error: len {1}->{2} \r\n to {4}", cmd.Value, argslen, service.ArgsType.Length, fiberRw.Async?.AcceptSocket?.RemoteEndPoint, service);
                            await SendError(id, $"call actor service:{cmd.Value} Args Error: len {argslen}->{service.ArgsType.Length}\r\n to {service}", ErrorType.ArgLenErr);
                            return false;
                        }

                    }
                    else
                    {
                        Log.WarnFormat("{1} call actor service:{0} not find cmd ", cmd.Value, fiberRw.Async?.AcceptSocket?.RemoteEndPoint);
                        await SendError(id, $"call actor service:{cmd.Value} not find the cmd,please check it", ErrorType.NotCmd);
                        return false;
                    }
                }
            }
            else
            {

                Log.WarnFormat("{0} call async service not read cmd,cmd is null", fiberRw.Async?.AcceptSocket?.RemoteEndPoint);
                await SendError(-1, "not read cmd", ErrorType.NotReadCmd);
                return false;
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
                            ActorRun.CallAction(id, cmd, args);
                            Dispose_table(memoryOwners);
                        }
                        break;
                    case 1:
                        {
                            await (Task)ActorRun.CallAsyncAction(id, cmd, args);
                            Dispose_table(memoryOwners);
                            await SendResult(id);
                        }
                        break;
                    case 2:
                        {
                            var ret_value = await ActorRun.CallAsyncFunc(id, cmd, args);
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

    
        private void Dispose_table(List<IMemoryOwner<byte>> memDisposableList)
        {
            if (memDisposableList.Count > 0)
            {
                foreach (var mem in memDisposableList)
                    mem.Dispose();
                memDisposableList.Clear();
            }
        }

        protected virtual async void RunCall(MethodRegister service, int cmd, long id, int runtype, List<IMemoryOwner<byte>> memoryOwners,  params object[] args)
        {
            try
            {
                var controller = await GetInstance(id, cmd, service.InstanceType);

           
                if (controller != null)
                    await RunControllerService(service, controller, id, runtype, memoryOwners, args);

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

        protected virtual async Task RunControllerService(MethodRegister service, AsyncController controller, long id, int runType, List<IMemoryOwner<byte>> memoryOwners, params object[] args)
        {
            switch (service.ReturnMode)
            {

                case ReturnTypeMode.Null:
                    if (runType == 0)
                    {
                        service.Method.Execute(controller, args);
                        Dispose_table(memoryOwners);
                        return;
                    }
                    break;
                case ReturnTypeMode.Task:
                    if (runType == 1)
                    {
                        await service.Method.ExecuteAsync(controller, args);
                        Dispose_table(memoryOwners);
                        await SendResult(id);
                        return;
                    }
                    break;
                case ReturnTypeMode.TaskValue:
                    if (runType == 2)
                    {
                        var ret_value = (object) await service.Method.ExecuteAsync(controller, args);
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

        protected virtual Task SendNotRunType(MethodRegister service, long id, int runtype)
        {
            Log.WarnFormat("{1} call async service:{0} not find RunType:{2} ", service, FiberRw.Async?.AcceptSocket?.RemoteEndPoint, runtype);
            return SendError(id, $"call async service:{service} not find RunType:{runtype}", ErrorType.NotRunType);
        }

        protected virtual async ValueTask<AsyncController> GetInstance(long id, int cmd, Type instanceType)
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
                                Log.WarnFormat("{1} call async service:{0}  not create instance from {2} Error:\r\n{3}", cmd, FiberRw.Async?.AcceptSocket?.RemoteEndPoint, instanceType.FullName, er.ToString());
                                await SendError(id, $"call async service:{cmd} not create instance from {instanceType.FullName} Error:{er.ToString()}", ErrorType.CreateInstanceErr);
                                return null;
                            }
                        }
                        else
                        {
                            Log.WarnFormat("{1} call async service:{0}  Constructor  not is public", cmd, FiberRw.Async?.AcceptSocket?.RemoteEndPoint);
                            await SendError(id, $"call async service:{cmd} Constructor not is public", ErrorType.ConstructorsErr);
                            return null;
                        }
                    }
                    else
                    {
                        Log.WarnFormat("{1} call async service:{0}  Constructor  not is Generic", cmd, FiberRw.Async?.AcceptSocket?.RemoteEndPoint);
                        await SendError(id, $"call async service:{cmd} Constructor not is Generic", ErrorType.ConstructorsErr);
                        return null;
                    }
                }
                else
                {
                    Log.WarnFormat("{1} call async service:{0} Constructor count error,need use 1 Constructor ", cmd, FiberRw.Async?.AcceptSocket?.RemoteEndPoint);
                    await SendError(id, $"call async service:{cmd} Constructor count error,need use 1 Constructor", ErrorType.ConstructorsErr);
                    return null;
                }
            }
            else
                return AsyncControllerInstanceDict[instanceType];

        }



    }
}
