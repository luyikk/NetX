using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using ZYSocket.Interface;

namespace Netx
{
    public static class SerializationPacker
    {
        /// <summary>
        /// 序列化接口,可以自定义实现序列化方法
        /// </summary>
        public static ISerialization? Serialization { get; set; }

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static byte[] PackSingleObject(object obj)
        {
 
            if (Serialization is null)
                throw new NullReferenceException("Serialization is null");

            return Serialization.Serialize(obj);
        }



        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="data">二进制数据</param>
        /// <param name="offset">偏移</param>
        /// <param name="length">长度</param>
        /// <returns></returns>       
        public static object UnpackSingleObject(Type type, byte[] data)
        {

            if (Serialization is null)
                throw new NullReferenceException("Serialization is null");

            return Serialization.Deserialize(type, data, 0, data.Length);
        }



        #region  return 字符串
        /// <summary>
        /// 读取内存流中一段字符串
        /// </summary>
        /// <param name="ms"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadString(byte[] Data)
        {
            string values = Encoding.UTF8.GetString(Data);
            return values;

        }
        #endregion
    }
}
