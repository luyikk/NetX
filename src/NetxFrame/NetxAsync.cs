using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Netx.Async;
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
            var ids = result.Id;

            if (AsyncResultDict.ContainsKey(ids))
            {
                var asyncback = AsyncResultDict[ids];

                if (AsyncResultDict.Remove(ids))
                {
                    asyncback.Completed(result);
                }
                else
                {
                    Log.ErrorFormat("not remove back ruest id:{0}", result.Id);
                }
            }
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

        protected virtual async Task ReadResultAsync(IFiberRw fiberRw)
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
                throw new NetxException($"data error:2500", ErrorType.ReadErr);
        }



    }
}
