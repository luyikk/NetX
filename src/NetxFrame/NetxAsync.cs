﻿using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources.Copy;
using ZYSocket;
using ZYSocket.FiberStream;

namespace Netx
{
    public abstract class NetxAsync : NetxBuffer
    {


        /// <summary>
        /// 异步结果返回,回调继续
        /// </summary>
        /// <param name="result"></param>
        protected virtual void AsyncBackResult(Result result)
        {
            if (AsyncResultDict.TryRemove(result.Id, out ManualResetValueTaskSource<Result> asyncback))
                asyncback.SetResult(result);
            else
            {
                if (result.IsError)
                {
                    try
                    {
                        Log.Error($"ErrorType:{(ErrorType)result.ErrorId} ErrMsg:\r\n{result.ErrorMsg}  ");
                    }
                    catch
                    {
                        Log.Error($"ErrorType:{result.ErrorId} ErrMsg:\r\n{result.ErrorMsg}  ");
                    }
                }
                else
                {
                    Log.ErrorFormat("not find back ruest id:{0}", result.Id);
                }
            }
        }


        protected virtual void ReadResult(ReadBytes read)
        {
            var id = read.ReadInt64();

            if (read.ReadBoolean()) //is error
            {
                AsyncBackResult(new Result()
                {
                    Id = id,
                    ErrorId = (read.ReadInt32()),
                    ErrorMsg = read.ReadString()
                });
            }
            else
            {
                var count = (read.ReadInt32());
                List<byte[]> args = new List<byte[]>(count);
                for (int i = 0; i < count; i++)
                {
                    args.Add(read.ReadArray());
                }

                AsyncBackResult(new Result(args)
                {
                    Id = id
                });

            }

        }

        protected virtual async Task ReadResultAsync(IFiberRw fiberRw)
        {
            var id = await fiberRw.ReadInt64();

            if (await fiberRw.ReadBoolean()) //is error
            {
                AsyncBackResult(new Result()
                {
                    Id = id,
                    ErrorId = await fiberRw.ReadInt32(),
                    ErrorMsg = await fiberRw.ReadString()
                });
            }
            else
            {
                var count = await fiberRw.ReadInt32();
                List<byte[]> args = new List<byte[]>(count);
                for (int i = 0; i < count; i++)
                    args.Add(await fiberRw.ReadArray());

                AsyncBackResult(new Result(args)
                {
                    Id = id
                });

            }


        }


        /// <summary>
        /// 处理超时请求
        /// </summary>
        public virtual void RequestTimeOutHandle()
        {
            while (RequestOutTimeQueue.Count > 0)
            {
                if (RequestOutTimeQueue.TryPeek(out RequestKeyTime keyTime))
                {
                    long outtime = RequestOutTime * 10000;

                    if ((TimeHelper.GetTime() - keyTime.Time) > outtime)
                    {
                        if (RequestOutTimeQueue.TryDequeue(out keyTime))
                        {
                            if (AsyncResultDict.ContainsKey(keyTime.Key))
                            {
                                Task.Factory.StartNew(() =>
                                {
                                    AsyncBackResult(new Result() { Id = keyTime.Key, ErrorMsg = "time out", ErrorId = (int)ErrorType.TimeOut });
                                });
                            }
                        }
                    }
                    else
                        break;

                }
                else
                    break;
            }
        }


    }
}
