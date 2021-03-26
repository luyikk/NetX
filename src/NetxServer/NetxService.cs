using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.FiberStream;
using ZYSocket.Server;

namespace Netx.Service
{
    public class NetxService : ServiceDecodeSetter, IDisposable
    {
        public ISocketServer SocketServer { get; set; }



        internal NetxService(IServiceProvider container)
            : base(container)
        {

            SocketServer = container.GetRequiredService<ISocketServer>();
            SocketServer.BinaryInput = new BinaryInputHandler(BinaryInputHandler);
            SocketServer.Connetions = new ConnectionFilter(ConnectionFilter);
            SocketServer.MessageInput = new DisconnectHandler(DisconnectHandler);

            foreach (var initialization in container.GetServices<Initialization>())
                initialization.initialize();

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
            this.Log.TraceFormat("IP Connect:{IPaddress}", socketAsync?.AcceptSocket?.RemoteEndPoint?.ToString() ?? "null");
            return true;
        }

        protected override void DisconnectHandler(string message, ISockAsyncEventAsServer socketAsync, int erorr)
        {
            base.DisconnectHandler(message, socketAsync, erorr);

            this.Log.TraceFormat("IP Disconnect:{IPAddress}", socketAsync?.AcceptSocket?.RemoteEndPoint?.ToString() ?? "null");
            if (socketAsync != null)
            {
                socketAsync.UserToken = null;
                socketAsync.AcceptSocket?.Dispose();
            }
        }


        protected virtual async void BinaryInputHandler(ISockAsyncEventAsServer socketAsync)
        {

            var fiberRw = await GetFiberRw(socketAsync);


            if (fiberRw == null)
            {
                socketAsync.Disconnect();
                return;
            }


            for (; ; )
            {
                try
                {
                    if (!await DataOnByLine(fiberRw))
                        break;
                }
                catch (System.Net.Sockets.SocketException) { break; }
                catch (System.IO.IOException) { break; }
                catch (Exception er)
                {
                    this.Log.Error("read error", er);
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
                    Log.TraceFormat("IP:{IPAddress} not verify key", fiberRw.Async?.AcceptSocket?.RemoteEndPoint.ToString() ?? "null");
                    SendToKeyError(fiberRw, true, "not verify key!");
                    fiberRw.UserToken = null;
                    return false;
                }

                var serviceName = await fiberRw.ReadString();

                if (!string.IsNullOrEmpty(ServiceOption.ServiceName))
                    if (!ServiceOption.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.TraceFormat("IP:{IPAddress} not find the service:{serviceName}", fiberRw.Async?.AcceptSocket?.RemoteEndPoint?.ToString() ?? "null", serviceName);
                        SendToKeyError(fiberRw, true, $"not find the service!{serviceName}");
                        return false;
                    }

                var key = await fiberRw.ReadString();
                if (!string.IsNullOrEmpty(OpenKey))
                {
                    if (string.Compare(OpenKey, key, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        Log.TraceFormat("IP:{IPAddress} verify key error:{key}", fiberRw.Async?.AcceptSocket?.RemoteEndPoint?.ToString() ?? "null", key);
                        SendToKeyError(fiberRw, true, "verify key error!");
                        return false;
                    }

                    SendToKeyError(fiberRw, msg: "verify success");

                    var session = await fiberRw.ReadInt64();
                    if (session == 0)
                        return await RunCreateToken(fiberRw);
                    else
                    {

                        if (ActorTokenDict.TryGetValue(session, out AsyncToken actorToken))
                            return await ResetToken(fiberRw, actorToken);
                        else
                        {
                            Log.TraceFormat("IP:{IPaddress} not find sessionid:{session}", fiberRw.Async?.AcceptSocket?.RemoteEndPoint?.ToString() ?? "null", session);
                            return await RunCreateToken(fiberRw);
                        }

                    }
                }
                else
                {

                    SendToKeyError(fiberRw, msg: "verify success");
                    var session = await fiberRw.ReadInt64();
                    if (session == 0)
                        return await RunCreateToken(fiberRw);
                    else
                    {

                        if (ActorTokenDict.TryGetValue(session, out AsyncToken actorToken))
                            return await ResetToken(fiberRw, actorToken);
                        else
                        {
                            Log.TraceFormat("IP:{IPAddress} not find sessionid:{session}", fiberRw.Async?.AcceptSocket?.RemoteEndPoint?.ToString() ?? "null", session);
                            return await RunCreateToken(fiberRw);
                        }

                    }
                }

            }
            else
            {
                Log.TraceFormat("IP:{IPAddress} token not null", fiberRw.Async?.AcceptSocket?.RemoteEndPoint?.ToString() ?? "null");
                SendToKeyError(fiberRw, true, "token not null error!");
                fiberRw.UserToken = null;
                return false;

            }
        }





    }


}
