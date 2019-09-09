using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.FiberStream;

namespace Netx.Service
{
    public abstract class AsyncBuffer : AsyncBase
    {
        public AsyncBuffer(IServiceProvider container, IFiberRw<AsyncToken> fiberRw, long sessionId) : 
            base(container, fiberRw, sessionId)
        {

        }

        /// <summary>
        /// 重载可以让客户端使用
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="Id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override async Task<IResult> AsyncFuncSend(int cmdTag, long Id, object[] args)
        {
            if (!IsConnect)
                if (!ConnectIt())
                    throw new NetxException("not connect", ErrorType.Notconnect);

            if (FiberRw != null)
            {
                
                //数据包格式为0000 0 0000  00000000 0000 .....
                //功能len(int)  标识(byte) 函数标识(int) 当前ids(long) 参数长度(int) 每个参数序列化后的数组
                using (var wr = new WriteBytes(FiberRw))
                {
                   

                    Task<int> WSend()
                    {
                        wr.WriteLen();
                        wr.Cmd(2400);
                        wr.Write((byte)2);
                        wr.Write(cmdTag);
                        wr.Write(Id);
                        wr.Write(args.Length);
                        foreach (var arg in args)
                        {
                            WriteObj(wr, arg);
                        }
                        return wr.Flush();
                    }

                    var result = GetResult(AddAsyncResult(Id));
                    await await FiberRw.Sync.Ask(WSend);
                    return await result;
                }

            }
            else
                throw new NullReferenceException("FiberRw is null!");
          
        }

        /// <summary>
        ///  重载可以让客户端使用
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="Id"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected override async Task SendAsyncAction(int cmdTag, long Id, object[] args)
        {
            if (!IsConnect)
                if (!ConnectIt())
                    throw new NetxException("not connect", ErrorType.Notconnect);

            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {
                    var result = GetResult(AddAsyncResult(Id));                 

                    Task<int> WSend()
                    {
                        wr.WriteLen();
                        wr.Cmd(2400);
                        wr.Write((byte)1);
                        wr.Write(cmdTag);
                        wr.Write(Id);
                        wr.Write(args.Length);
                        foreach (var arg in args)
                        {
                            WriteObj(wr, arg);
                        }
                        return wr.Flush();
                    }

                    await await FiberRw.Sync.Ask(WSend);

                    var res = await result;

                    if (res.IsError)
                    {
                        throw new NetxException(res.ErrorMsg, res.ErrorId);
                    }
                }

            }
            else
                Log.Error("Send fail,is not fiber");

        }

        /// <summary>
        ///  重载可以让客户端使用
        /// </summary>
        /// <param name="cmdTag"></param>
        /// <param name="args"></param>
        protected override void SendAction(int cmdTag, object[] args)
        {
            if (!IsConnect)
                if (!ConnectIt())
                    throw new NetxException("not connect", ErrorType.Notconnect);

            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {
                   
                    void WSend()
                    {
                        wr.WriteLen();
                        wr.Cmd(2400);
                        wr.Write((byte)0);
                        wr.Write(cmdTag);
                        wr.Write((long)-1);
                        wr.Write(args.Length);
                        foreach (var arg in args)
                        {
                            WriteObj(wr, arg);
                        }

                        wr.Flush();
                    }
                    

                    FiberRw.Sync.Tell(WSend);
                }
            }
            else
                Log.Error("Send fail,is not fiber");

        }


        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual async Task SendResult(long id, object argument)
        {
            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {                  
                    Task<int> WSend()
                    {
                        wr.WriteLen();
                        wr.Cmd(2500);
                        wr.Write(id);
                        wr.Write(false);
                        wr.Write(1);
                        wr.Write(SerializationPacker.PackSingleObject(argument));
                        return wr.Flush();
                    }

                    await await FiberRw.Sync.Ask(WSend);
                }
            }
            else
            {
                Log.Error("Send fail,is not fiber");               
            }

        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual async Task SendResult(long id, byte[][] arguments = null)
        {
            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {
                   
                    Task<int> WSend()
                    {
                        wr.WriteLen();
                        wr.Cmd(2500);
                        wr.Write(id);
                        wr.Write(false);
                        if (arguments is null)
                            wr.Write(0);
                        else
                        {
                            wr.Write(arguments.Length);
                            foreach (var item in arguments)
                                wr.Write(item);
                        }
                        return  wr.Flush();
                    }

                    await await FiberRw.Sync.Ask(WSend);
                }
            }
            else
            {
                Log.Error("Send fail,is not fiber");               
            }

        }

        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected virtual async Task SendResult(Result result)
        {
            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {
                  
                    Task<int> WSend()
                    {
                        wr.WriteLen(); //为了兼容其他框架和其他的语言,还是发个长度吧
                        wr.Cmd(2500);
                        wr.Write(result.Id);

                        if (result.IsError)
                        {
                            wr.Write(true);
                            wr.Write(result.ErrorId);
                            wr.Write(result.ErrorMsg);
                        }
                        else
                        {
                            wr.Write(false);
                            wr.Write(result.Arguments.Count);
                            foreach (var item in result.Arguments)
                                wr.Write(item);
                        }
                        return wr.Flush();
                    }

                    await await FiberRw.Sync.Ask(WSend);
                }
            }
            else
            {
                Log.Error("Send fail,is not fiber");               
            }

        }

        /// <summary>
        /// 发送错误
        /// </summary>
        /// <param name="id"></param>
        /// <param name="msg"></param>
        /// <param name="errorType"></param>
        /// <returns></returns>
        protected virtual async Task SendError(long id, string msg, ErrorType errorType)
        {

            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {
                    Task<int> WSend()
                    {
                        wr.WriteLen();
                        wr.Cmd(2500);
                        wr.Write(id);
                        wr.Write(true);
                        wr.Write((int)errorType);
                        wr.Write(msg);
                        return wr.Flush();
                    }

                    await await FiberRw.Sync.Ask(WSend);
                }
            }
            else
            {
                Log.Error("Send fail,is not fiber");               
            }
        }

        /// <summary>
        /// 发送Session
        /// </summary>
        /// <returns></returns>
        public virtual async Task SendSessionId()
        {
            if (FiberRw != null)
            {
                using (var wr = new WriteBytes(FiberRw))
                {
                    Task<int> WSend()
                    {
                        wr.WriteLen();
                        wr.Cmd(2000);
                        wr.Write(SessionId);

                        return wr.Flush();
                    }

                    await await FiberRw.Sync.Ask(WSend);
                }
            }
            else
            {
                Log.Error("Send fail,is not fiber");               
            }
        }


        protected virtual Task SendNotRunType(MethodRegister service, long id, int runtype)
        {
            Log.WarnFormat("{1} call async service:{0} not find RunType:{2} ", service, FiberRw.Async?.AcceptSocket?.RemoteEndPoint, runtype);
            return SendError(id, $"call async service:{service} not find RunType:{runtype}", ErrorType.NotRunType);
        }

    }
}
