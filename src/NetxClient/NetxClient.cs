using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.Client;

namespace Netx.Client
{
    /// <summary>
    /// NetX 客户端
    /// </summary>
    public class NetxSClient : NetxDecodeSetter, INetxSClient
    {
        private bool Disposed = false;

        public SocketClient SocketClient { get; private set; }

        public event DisconnectHandler? Disconnect;

        internal NetxSClient(IServiceProvider container)
            : base(container)
        {
            SocketClient = Container.GetRequiredService<SocketClient>();
            SocketClient.BinaryInput += SocketClient_BinaryInput;
            SocketClient.Disconnect += SocketClient_Disconnect;
        }

        private void Init()
        {
            SocketClient = Container.GetRequiredService<SocketClient>();
            SocketClient.BinaryInput += SocketClient_BinaryInput;
            SocketClient.Disconnect += SocketClient_Disconnect;
        }

        public void Open()
        {
            var timeout = ConnectOption.ConnectedTimeOut;
            var result = SocketClient.Connect(ConnectOption.Host, ConnectOption.Port, timeout);
            if (!result.IsSuccess)
            {
                throw new NetxException(result.Msg, ErrorType.ConnectErr);
            }
        }

        public async Task<ConnectResult> OpenAsync()
        {
            var timeout = ConnectOption.ConnectedTimeOut;
            return await SocketClient.ConnectAsync(ConnectOption.Host, ConnectOption.Port, timeout);
        }

        public void Close()
        {
            try
            {
                IsConnect = false;
                SocketClient?.ShutdownBoth();
                SocketClient?.Dispose();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        protected override bool ConnectIt()
        {
            if (Disposed)
                return false;

            Init();

            try
            {
                Open();
                return true;
            }
            catch (NetxException er)
            {
                Log.Error("connect error:", er);
                return false;
            }
        }


        private void SocketClient_Disconnect(ISocketClient client, ISockAsyncEventAsClient socketAsync, string msg)
        {
            Log.Info($"{ConnectOption.Host}:{ConnectOption.Port}->{msg}");
            Close();
            Disconnect?.Invoke(client, socketAsync, msg);
        }



        private async void SocketClient_BinaryInput(ISocketClient client, ISockAsyncEventAsClient socketAsync)
        {
            var fiberRw = await GetFiberRw(socketAsync);

            try
            {

                if (fiberRw == null)
                {
                    client.SetConnected(false, "ssl error");
                    return;
                }

                IWrite = fiberRw;

                if (!isConnect)
                {

                    await SendVerify(); //发送KEY和sessionid验证

                    using ReadBytes read = new ReadBytes(fiberRw);
                    await read.Init();

                    switch (read.ReadInt32())
                    {
                        case 1000: //key check
                            {
                                var iserror = read.ReadBoolean();

                                if (!iserror)
                                {
                                    Log.Trace(read.ReadString());

                                    if (read.Memory.Length >= 1)                                    
                                        if (read.ReadByte() == 1)                                        
                                            Mode = 1;

                                    isConnect = true;
                                    client.SetConnected();
                                    await ReadIng(fiberRw);
                                }
                                else
                                {
                                    var msg = read.ReadString();
                                    Log.Info(msg);
                                    client.SetConnected(false, msg);
                                }

                            }
                            break;
                    }



                }

                client.ShutdownBoth();

            }
            catch (Exception er)
            {
                if (!client.IsConnect)
                {
                    client.SetConnected(false, er.Message);
                }
                else
                {
                    Log.Error(er);
                    client.ShutdownBoth();
                }
            }
        }

        public void Dispose()
        {
            Disposed = true;
            Close();
        }
    }
}
