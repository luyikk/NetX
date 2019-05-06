using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZYSocket.FiberStream;

namespace Netx.Client
{
    public abstract class NetxAnalysis : NetxAsyncCaller
    {
        public NetxAnalysis(IServiceProvider container)
            : base(container)
        {

        }

        protected virtual async Task ReadIng(IFiberRw fiberRw)
        {
            while (isConnect)
            {
                try
                {
                    await DataOnByLine(fiberRw);
                }
                catch (System.Net.Sockets.SocketException)
                {
                    break;
                }
                catch (Exception er)
                {
                    Log.Error(this, er);
                    break;
                }
            }

        }


        protected virtual async Task DataOnByLine(IFiberRw fiberRw)
        {
            await fiberRw.ReadInt32();
            var cmd = (await fiberRw.ReadInt32());

            switch (cmd)
            {
                case 2000: //set session
                    {
                        var sessionid = (await fiberRw.ReadInt64()).GetValueOrDefault(0);
                        Log.TraceFormat("save sessionid {0}", sessionid);
                        Session.SaveSessionId(sessionid);
                    }
                    break;
                case 2500: //set result
                    {
                        var id = await fiberRw.ReadInt64();

                        if (id.HasValue)
                        {
                            if ((await fiberRw.ReadBoolean()).Value) //is error
                            {
                                AsyncBackResult(new Result()
                                {
                                    Id = id.Value,
                                    ErrorId = (await fiberRw.ReadInt32()).Value,
                                    ErrorMsg = await fiberRw.ReadString()
                                });
                            }
                            else
                            {
                                var count = (await fiberRw.ReadInt32()).Value;
                                List<byte[]> args = new List<byte[]>(count);
                                for (int i = 0; i < count; i++)
                                    args.Add(await fiberRw.ReadArray());

                                AsyncBackResult(new Result(args)
                                {
                                    Id = id.Value
                                });                           

                            }

                        }
                        else
                            throw new NetxException($"data error:{cmd.GetValueOrDefault()}", ErrorType.ReadErr);
                    }
                    break;
                default:
                    throw new NetxException($"data error:{cmd.GetValueOrDefault()}", ErrorType.ReadErr);
            }

        }
    }
}
