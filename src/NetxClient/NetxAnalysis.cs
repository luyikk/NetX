using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZYSocket.FiberStream;

namespace Netx.Client
{
    public abstract class NetxAnalysis : NetxClientCalling
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
                case 2400: //Call It
                    {
                        await Calling(fiberRw);
                    }
                    break;
                case 2500: //set result
                    {
                        await ReadResultAsync(fiberRw);
                    }
                    break;
                default:
                    throw new NetxException($"data error:{cmd.GetValueOrDefault()}", ErrorType.ReadErr);
            }

        }
    }
}
