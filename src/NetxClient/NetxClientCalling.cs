using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.FiberStream;

namespace Netx.Client
{
    public abstract  class NetxClientCalling: NetxAsyncRegisterInstance,INetxSClient
    {
        public NetxClientCalling(IServiceProvider container)
          : base(container)
        {

        }

        protected async Task Calling(IFiberRw fiberRw)
        {
            var type = await fiberRw.ReadByte();
            switch (type)
            {
                case 0: //RUN CALL NOT RES
                    {
                        await DataOnByRead(fiberRw, 0);
                    }
                    break;
                case 1: //RUN CALL HAVE RES
                    {
                        await DataOnByRead(fiberRw, 1);
                    }
                    break;
                case 2: // RUN CALL RETURN 
                    {
                        await DataOnByRead(fiberRw, 2);
                    }
                    break;
                default:
                    throw new NetxException($"not is call type{type}", ErrorType.CallErr);
            }

        }

        private async Task DataOnByRead(IFiberRw fiberRw, byte runtype)
        {
            var cmd = await fiberRw.ReadInt32();
            if (cmd.HasValue)
            {

                var id = (await fiberRw.ReadInt64()).GetValueOrDefault(-1);
                if (MethodInstanceDict.TryGetValue(cmd.Value, out InstanceRegister service))
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

                        RunCall(service, cmd.Value, id, runtype, mem_disposetable, args);                      
                    }
                    else
                    {
                        Log.WarnFormat("call method tag :{0} Args Error: len {1}->{2}  to\r\n  {3}", cmd.Value, argslen, service.ArgsType.Length, service);
                        await SendError(id, $"call method tag :{ cmd.Value} Args Error: len {argslen}->{service.ArgsType.Length}  to\r\n  {service}", ErrorType.ArgLenErr);                   
                    }
                }


            }
            else
            {
                Log.WarnFormat("call read cmd error");
                await SendError(-1, "not read cmd", ErrorType.NotReadCmd);               
            }
        }

        private async void RunCall(InstanceRegister service,int cmd,long id,byte runType, List<IMemoryOwner<byte>> memoryOwners, object[] args)
        {
            try
            {
                switch (service.ReturnMode)
                {

                    case ReturnTypeMode.Null:
                        if (runType == 0)
                        {
                            if (service.Instance is IMethodController controller)
                                controller.current = this;

                            service.Method.Execute(service.Instance, args);
                            Dispose_table(memoryOwners);
                            return;
                        }
                        break;
                    case ReturnTypeMode.Task:
                        if (runType == 1)
                        {
                            if (service.Instance is IMethodController controller)
                                controller.current = this;

                            await service.Method.ExecuteAsync(service.Instance, args);
                            Dispose_table(memoryOwners);
                            await SendResult(id);
                            return;
                        }
                        break;
                    case ReturnTypeMode.TaskValue:
                        if (runType == 2)
                        {
                            if (service.Instance is IMethodController controller)
                                controller.current = this;

                            var ret_value = (object)await service.Method.ExecuteAsync(service.Instance, args);
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
            catch (System.Net.Sockets.SocketException)
            {

            }
            catch (NetxException er)
            {               
                Log.Error(er);
                await SendError(id, $"Client Method Tag:{cmd} Call Err:{er.Message}", ErrorType.CallErr);
            }
            catch (Exception er)
            {
                Log.Error(er);
                await SendError(id, $"Client Method Tag:{cmd} Call Err:{er.Message}", ErrorType.CallErr);
            }
        }

        protected virtual Task SendNotRunType(MethodRegister service, long id, int runtype)
        {
            Log.WarnFormat("call method:{0} not find runtype:{1} ", service, runtype);
            return SendError(id, $"call method:{service}  not find runtype:{runtype}", ErrorType.NotRunType);
        }



    }
}
