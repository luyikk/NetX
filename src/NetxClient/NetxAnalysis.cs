using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZYSocket;
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

            using (ReadBytes read = new ReadBytes(fiberRw))
            {
                await read.Init();
                var cmd = read.ReadInt32();

                switch (cmd)
                {
                    case 2000: //set session
                        {
                            var sessionid = read.ReadInt64();
                            Log.TraceFormat("save sessionid {0}", sessionid);
                            Session.SaveSessionId(sessionid);
                        }
                        break;
                    case 2400: //Call It
                        {
                            await Calling(read);
                        }
                        break;
                    case 2500: //set result
                        {
                             ReadResult(read);
                        }
                        break;
                    default:
                        throw new NetxException($"data error:{cmd}", ErrorType.ReadErr);
                }

            }
        }
    }
}
