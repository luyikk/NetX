using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Netx.Actor;
using System;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.FiberStream;
using ZYSocket.FiberStream.Synchronization;
using ZYSocket.Server;

namespace Netx.Service
{
    public class NetxService : ServiceSslSetter, IDisposable
    {
        public ISocketServer SocketServer { get; set; }

       

        internal NetxService(IServiceProvider container)
            : base(container)
        {
         
            SocketServer = container.GetRequiredService<ISocketServer>();
            SocketServer.BinaryInput = new BinaryInputHandler(BinaryInputHandler);
            SocketServer.Connetions = new ConnectionFilter(ConnectionFilter);
            SocketServer.MessageInput = new DisconnectHandler(DisconnectHandler);
           
        }

        public void Dispose()
        {
            SocketServer?.Dispose();
        }

        public void Start()
        {
            Log.Info("NetxService Start");
            SocketServer.Start();
        }

        public void Stop()
        {
            Log.Info("NetxService Stop");
            SocketServer.Stop();
        }


        private bool ConnectionFilter(ISockAsyncEventAsServer socketAsync)
        {
            this.Log.Trace($"IP Connect:{socketAsync?.AcceptSocket?.RemoteEndPoint}");
            return true;
        }

        protected override void DisconnectHandler(string message, ISockAsyncEventAsServer socketAsync, int erorr)
        {
            base.DisconnectHandler(message, socketAsync, erorr);

            this.Log.TraceFormat("IP Disconnect:{0}", socketAsync?.AcceptSocket?.RemoteEndPoint); 
            socketAsync.UserToken = null;
            socketAsync.AcceptSocket.Dispose();
        }


        protected virtual async void BinaryInputHandler(ISockAsyncEventAsServer socketAsync)
        {

            var fiberRw = await GetFiberRw(socketAsync);


            if (fiberRw == null)
            {
                socketAsync.Disconnect();
                return;
            }

            fiberRw.Sync = new SyncRun();


            for (; ; )
            {
                try
                {
                    if (!await DataOnByLine(fiberRw))
                        break;
                }
                catch (System.Net.Sockets.SocketException) { break; }
                catch (Exception er)
                {
                    this.Log.Error(fiberRw, er);
                    break;
                }
            }

            socketAsync.Disconnect();
        }

        protected async Task<bool> DataOnByLine(IFiberRw<AsyncToken> fiberRw)
        {
            if (fiberRw.UserToken is null)
            {
                var cmd = await fiberRw.ReadInt32();
                if (cmd != 1000)
                {
                    Log.TraceFormat("IP:{0} not verify key", fiberRw.Async?.AcceptSocket?.RemoteEndPoint);
                    await SendToKeyError(fiberRw, true, "not verify key!");
                    fiberRw.UserToken = null;
                    return false;
                }

                var serviceName = await fiberRw.ReadString();

                if (!string.IsNullOrEmpty(ServiceOption.ServiceName))
                    if (!ServiceOption.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.TraceFormat("IP:{0} not find the service:{1}", fiberRw.Async?.AcceptSocket?.RemoteEndPoint, serviceName);
                        await SendToKeyError(fiberRw, true, $"not find the service!{serviceName}");
                        return false;
                    }                  

                var key = await fiberRw.ReadString();
                if (!String.IsNullOrEmpty(OpenKey))
                {
                    if (string.Compare(OpenKey, key, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        Log.TraceFormat("IP:{0} verify key error:{1}", fiberRw.Async?.AcceptSocket?.RemoteEndPoint, key);
                        await SendToKeyError(fiberRw, true, "verify key error!");
                        return false;
                    }

                    await SendToKeyError(fiberRw, msg: "verify success");

                    var session = await fiberRw.ReadInt64();
                    if (session == 0)
                        return await RunCreateToken(fiberRw);
                    else
                    {

                        if (ActorTokenDict.TryGetValue(session, out AsyncToken actorToken))
                            return await ResetToken(fiberRw, actorToken);
                        else
                        {
                            Log.TraceFormat("IP:{0} not find sessionid:{1}", fiberRw.Async?.AcceptSocket?.RemoteEndPoint, session);
                            return await RunCreateToken(fiberRw);
                        }

                    }
                }
                else
                {

                    await SendToKeyError(fiberRw, msg: "verify success");
                    var session = await fiberRw.ReadInt64();
                    if (session == 0)
                        return await RunCreateToken(fiberRw);
                    else
                    {

                        if (ActorTokenDict.TryGetValue(session, out AsyncToken actorToken))
                            return await ResetToken(fiberRw, actorToken);
                        else
                        {
                            Log.TraceFormat("IP:{0} not find sessionid:{1}", fiberRw.Async?.AcceptSocket?.RemoteEndPoint, session);
                            return await RunCreateToken(fiberRw);
                        }

                    }
                }

            }
            else
            {
                Log.TraceFormat("IP:{0} token not null", fiberRw.Async?.AcceptSocket?.RemoteEndPoint);
                await SendToKeyError(fiberRw, true, "token not null error!");
                fiberRw.UserToken = null;
                return false;

            }
        }





    }


}
