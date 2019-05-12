using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ZYSocket.FiberStream;
using ZYSocket;
using System.Buffers;
using Netx.Async;

namespace Netx
{
    public abstract class NetxBuffer:NetxBase
    {

        /// <summary>
        /// ZYSOCKET V 的fiberRw 对象用于发送接收数据
        /// </summary>
        protected IBufferWrite IWrite { get; set; }

        protected bool isConnect;
        /// <summary>
        /// 当前是否连接
        /// </summary>
        public bool IsConnect { get=> isConnect; protected set=> isConnect=value; }

        /// <summary>
        /// 异步请求发送,将异步返回结果
        /// </summary>
        /// <param name="cmdTag">命令</param>
        /// <param name="Id">当前批次Id用于标识批次,确保唯一</param>
        /// <param name="args">参数</param>
        /// <returns></returns>
        protected async override Task<IResult> AsyncFuncSend(int cmdTag, long Id, object[] args)
        {
           
            if (!IsConnect)
                if(!ConnectIt())
                    throw new NetxException("not connect", ErrorType.Notconnect);

            //数据包格式为 0 0000  00000000 0000 .....
            //功能标识(byte) 函数标识(int) 当前ids(long) 参数长度(int) 每个参数序列化后的数组
            IWrite.Write(2400);
            IWrite.Write((byte)2); 
            IWrite.Write(cmdTag);
            IWrite.Write(Id);
            IWrite.Write(args.Length);
            foreach (var arg in args)
            {
                WriteObj(IWrite, arg);
            }

            var result = GetResult(AddAsyncResult(Id));

            await IWrite.Flush();

            if (result.IsCompleted)
                return result.Result;
            else
                return await result;
        }      

        private async ValueTask<IResult> GetResult(AsyncResultAwaiter<Result> asyncResult)
        {
            return await asyncResult;
        }

        /// <summary>
        /// 发送调用求情
        /// </summary>
        /// <param name="cmdTag">命令</param>
        /// <param name="args">参数</param>
        protected override void SendAction(int cmdTag, object[] args)
        {
            if (!IsConnect)
                if (!ConnectIt())
                    throw new NetxException("not connect", ErrorType.Notconnect);

            IWrite.Write(2400);
            IWrite.Write((byte)0);
            IWrite.Write(cmdTag);
            IWrite.Write((long)-1);
            IWrite.Write(args.Length);
            foreach (var arg in args)
            {
                WriteObj(IWrite, arg);
            }
            IWrite.Flush();
        }

        /// <summary>
        /// 发送调用请求,异步等待返回
        /// </summary>
        /// <param name="cmdTag">命令</param>
        /// <param name="Id">Id</param>
        /// <param name="args">参数</param>
        /// <returns>异步等待Task</returns>
        protected async  override Task SendAsyncAction(int cmdTag, long Id, object[] args)
        {
            if (!IsConnect)
                if (!ConnectIt())
                    throw new NetxException("not connect", ErrorType.Notconnect);

            IWrite.Write(2400);
            IWrite.Write((byte)1);
            IWrite.Write(cmdTag);
            IWrite.Write(Id);
            IWrite.Write(args.Length);
            foreach (var arg in args)
            {
                WriteObj(IWrite, arg);
            }

            var result = GetResult(AddAsyncResult(Id));

            await IWrite.Flush();

            if (result.IsCompleted)
            {
                var res = result.Result;
                if (res.IsError)
                    throw new NetxException(res.ErrorMsg, res.ErrorId);
            }
            else
            {
                var res = await result;
                if (res.IsError)                
                    throw new NetxException(res.ErrorMsg, res.ErrorId);                
            }
        }
         

        protected abstract bool ConnectIt();


        /// <summary>
        /// 将参数判断类型后再写入相应的方法来序列化
        /// </summary>
        /// <param name="bufferWrite">fiberRw 对象</param>
        /// <param name="p">object类型参数</param>
        protected virtual void WriteObj(IBufferWrite bufferWrite, object p)
        {
            switch (p)
            {
                case sbyte a:
                    bufferWrite.Write((byte)a);                    
                    break;
                case byte a:
                    bufferWrite.Write(a);
                    break;
                case short a:
                    bufferWrite.Write(a);
                    break;
                case ushort a:
                    bufferWrite.Write(a);
                    break;
                case int a:
                    bufferWrite.Write(a);
                    break;
                case uint a:
                    bufferWrite.Write(a);
                    break;
                case long a:
                    bufferWrite.Write(a);
                    break;
                case ulong a:
                    bufferWrite.Write(a);
                    break;
                case float a:
                    bufferWrite.Write(a);
                    break;
                case double a:
                    bufferWrite.Write(a);
                    break;
                case decimal a:
                    bufferWrite.Write(Convert.ToDouble(a));
                    break;
                case bool a:
                    bufferWrite.Write(a);
                    break;
                case byte[] a:
                    bufferWrite.Write(a);
                    break;
                case string a:
                    bufferWrite.Write(a);
                    break;
                case Memory<byte> a:
                    bufferWrite.Write(a);
                    break;
                case ArraySegment<byte> a:
                    bufferWrite.Write(a);
                    break;
                default:
                    bufferWrite.Write(p);
                    break;

            }
        }


        protected virtual object  ReadData(ReadBytes read, Type type)
        {

            if (type == typeof(sbyte))
                return (sbyte)read.ReadByte();
            else if (type == typeof(byte))
                return read.ReadByte();
            else if (type == typeof(short))
                return read.ReadInt16();
            else if (type == typeof(ushort))
                return read.ReadUint16();
            else if (type == typeof(int))
                return read.ReadInt32();
            else if (type == typeof(uint))
                return read.ReadUint32();
            else if (type == typeof(long))
                return read.ReadInt64();
            else if (type == typeof(ulong))
                return read.ReadUint64();
            else if (type == typeof(double))
                return read.ReadDouble();
            else if (type == typeof(decimal))
                return Convert.ToDecimal(read.ReadDouble());
            else if (type == typeof(bool))
                return read.ReadBoolean();
            else if (type == typeof(byte[]))
                return read.ReadArray();
            else if (type == typeof(string))
                return read.ReadString();
            else if (type == typeof(Memory<byte>))            
               return read.ReadMemory();              
            else if (type == typeof(ArraySegment<byte>))
            {
                var mem =  read.ReadMemory();
                return mem.GetArray();
            }
            else
                return  read.ReadObject(type);

        }


        protected virtual async Task<(object arg, IMemoryOwner<byte> ownew)> ReadDataAsync(IBufferAsyncRead fiberR,Type type)
        {
            
            if (type == typeof(sbyte))            
                return ((sbyte)await fiberR.ReadByte(),null);            
            else if (type == typeof(byte))            
                return (await fiberR.ReadByte(), null);            
            else if (type == typeof(short))            
                return (await fiberR.ReadInt16(), null);            
            else if (type == typeof(ushort))            
                return (await fiberR.ReadUInt16(), null);            
            else if (type == typeof(int))            
                return (await fiberR.ReadInt32(), null);            
            else if (type == typeof(uint))            
                return (await fiberR.ReadUInt32(), null);            
            else if (type == typeof(long))            
                return (await fiberR.ReadInt64(), null);            
            else if (type == typeof(ulong))            
                return (await fiberR.ReadUInt64(), null);            
            else if (type == typeof(double))            
                return (await fiberR.ReadDouble(), null);             
            else if (type == typeof(decimal))            
                return (Convert.ToDecimal(await fiberR.ReadDouble()),null);            
            else if (type == typeof(bool))            
                return (await fiberR.ReadBoolean(),null);            
            else if (type == typeof(byte[]))            
                return (await fiberR.ReadArray(),null);            
            else if (type == typeof(string))            
                return (await fiberR.ReadString(),null);       
            else if (type == typeof(Memory<byte>))
            {
                var mem= await fiberR.ReadMemory();
                return (mem.Value, mem.MemoryOwner);
            }
            else if (type == typeof(ArraySegment<byte>))
            {
                var mem = await fiberR.ReadMemory();               
                return (mem.Value.GetArray(),mem.MemoryOwner);
            }
            else
                return (await fiberR.ReadObject(type),null);

        }


    }
}
