using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Netx.Async;

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
      
    }
}
