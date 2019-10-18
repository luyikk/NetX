using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ZYSocket;
using ZYSocket.FiberStream;
using Microsoft.Extensions.DependencyInjection;

namespace Netx.Client
{
    public abstract class NetxClientCalling : NetxAsyncRegisterInstance, INetxSClient
    {
        public NetxClientCalling(IServiceProvider container)
          : base(container)
        {

        }
        public ILogger GetLogger<T>()
        {
            return Container.GetRequiredService<ILogger<T>>();

        }



        protected async Task Calling(ReadBytes read)
        {
            var type = read.ReadByte();
            switch (type)
            {
                case 0: //RUN CALL NOT RES
                    {
                        await DataOnByRead(read, 0);
                    }
                    break;
                case 1: //RUN CALL HAVE RES
                    {
                        await DataOnByRead(read, 1);
                    }
                    break;
                case 2: // RUN CALL RETURN 
                    {
                        await DataOnByRead(read, 2);
                    }
                    break;
                default:
                    throw new NetxException($"not is call type{type}", ErrorType.CallErr);
            }

        }

        private async Task DataOnByRead(ReadBytes read, byte runtype)
        {
            var cmd = read.ReadInt32();
            var id = read.ReadInt64();
            if (MethodInstanceDict.TryGetValue(cmd, out InstanceRegister service))
            {
                var argslen = read.ReadInt32();
                if (argslen == service.ArgsLen)
                {
                    object[] args = new object[argslen];
                    for (int i = 0; i < argslen; i++)
                        args[i] = base.ReadData(read, service.ArgsType[i]);

                    RunCall(service, cmd, id, runtype, args);
                }
                else
                {
                    Log!.WarnFormat($"call method tag :{cmd} Args Error: len {argslen}->{service.ArgsType.Length}  to\r\n  {service}");
                    await SendError(id, $"call method tag :{ cmd} Args Error: len {argslen}->{service.ArgsType.Length}  to\r\n  {service}", ErrorType.ArgLenErr);
                }
            }
        }

        private async void RunCall(InstanceRegister service, int cmd, long id, byte runType, object[] args)
        {
            try
            {
                switch (service.ReturnMode)
                {

                    case ReturnTypeMode.Null:
                        if (runType == 0)
                        {
                            if (service.Instance is IMethodController controller)
                                controller.Current = this;

                            service.Method.Execute(service.Instance, args);

                            return;
                        }
                        break;
                    case ReturnTypeMode.Task:
                        if (runType == 1)
                        {
                            if (service.Instance is IMethodController controller)
                                controller.Current = this;

                            await service.Method.ExecuteAsync(service.Instance, args);

                            await SendResult(id);
                            return;
                        }
                        break;
                    case ReturnTypeMode.TaskValue:
                        if (runType == 2)
                        {
                            if (service.Instance is IMethodController controller)
                                controller.Current = this;

                            var ret_value = (object)await service.Method.ExecuteAsync(service.Instance, args);

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
                Log!.Error(er);
                await SendError(id, $"Client Method Tag:{cmd} Call Err:{er.Message}", ErrorType.CallErr);
            }
            catch (Exception er)
            {
                Log!.Error(er);
                await SendError(id, $"Client Method Tag:{cmd} Call Err:{er.Message}", ErrorType.CallErr);
            }
        }

        protected virtual Task SendNotRunType(MethodRegister service, long id, int runtype)
        {
            Log!.WarnFormat("call method:{0} not find runtype:{1} ", service, runtype);
            return SendError(id, $"call method:{service}  not find runtype:{runtype}", ErrorType.NotRunType);
        }

    }

}
