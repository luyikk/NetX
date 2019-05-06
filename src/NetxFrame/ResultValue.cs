using System;
using System.Collections.Generic;
using System.Text;

namespace Netx
{
  

    /// <summary>
    /// 返回值
    /// </summary>
    public class ResultValue : IResultValue
    {
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; private set; }

        public ResultValue(byte[] data)
        {
            this.Data = data;
        }

        public T Value<T>()
        {
            return (T)SerializationPacker.UnpackSingleObject(typeof(T), Data);
        }


    }
}
