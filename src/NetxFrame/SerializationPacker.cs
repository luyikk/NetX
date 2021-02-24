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

            //switch (obj)
            //{
            //    case string arg:
            //        return Encoding.UTF8.GetBytes(arg);
            //    case byte[] arg:
            //        return arg;
            //    case sbyte arg:
            //        return new byte[] { (byte)arg };
            //    case bool arg:
            //        return BitConverter.GetBytes(arg);
            //    case byte arg:
            //        return new byte[] { arg };
            //    case short arg:
            //        return BitConverter.GetBytes(arg);
            //    case ushort arg:
            //        return BitConverter.GetBytes(arg);
            //    case int arg:
            //        return BitConverter.GetBytes(arg);
            //    case uint arg:
            //        return BitConverter.GetBytes(arg);
            //    case long arg:
            //        return BitConverter.GetBytes(arg);
            //    case ulong arg:
            //        return BitConverter.GetBytes(arg);
            //    case float arg:
            //        return BitConverter.GetBytes(arg);
            //    case double arg:
            //        return BitConverter.GetBytes(arg);
            //    case decimal arg:
            //        return BitConverter.GetBytes(Convert.ToDouble(arg));
            //    case Array array:
            //        {
            //            if (Serialization is null)
            //                throw new NullReferenceException("Serialization is null");

            //            List<byte[]> arlist = new List<byte[]>(array.Length);
            //            for (int i = 0; i < array.Length; i++)
            //                arlist.Add(PackSingleObject(array.GetValue(i)));


            //            return Serialization.Serialize(arlist);
            //        }
            //    default:
            //        {
            //            if (Serialization is null)
            //                throw new NullReferenceException("Serialization is null");

            //            return Serialization.Serialize(obj);
            //        }

            //}

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


            //if (type == typeof(string))
            //{
            //    return ReadString(data);
            //}
            //else if (type == typeof(byte[]))
            //{
            //    return data;
            //}
            //else if (type == typeof(bool))
            //{
            //    return BitConverter.ToBoolean(data, 0);
            //}
            //else if (type == typeof(byte))
            //{
            //    return data[0];
            //}
            //else if (type == typeof(sbyte))
            //{
            //    unchecked
            //    {
            //        return (sbyte)data[0];
            //    }
            //}
            //else if (type == typeof(short))
            //{
            //    return BitConverter.ToInt16(data, 0);
            //}
            //else if (type == typeof(ushort))
            //{
            //    return BitConverter.ToUInt16(data, 0);
            //}
            //else if (type == typeof(int))
            //{
            //    return BitConverter.ToInt32(data, 0);
            //}
            //else if (type == typeof(uint))
            //{
            //    return BitConverter.ToUInt32(data, 0);
            //}
            //else if (type == typeof(long))
            //{
            //    return BitConverter.ToInt64(data, 0);
            //}
            //else if (type == typeof(ulong))
            //{
            //    return BitConverter.ToUInt64(data, 0);
            //}
            //else if (type == typeof(float))
            //{
            //    return BitConverter.ToSingle(data, 0);

            //}
            //else if (type == typeof(double))
            //{
            //    return BitConverter.ToDouble(data, 0);
            //}
            //else if (type == typeof(decimal))
            //{
            //    return Convert.ToDecimal(BitConverter.ToDouble(data, 0));
            //}
            //else if (type.BaseType == typeof(Array))
            //{
            //    if (Serialization is null)
            //        throw new NullReferenceException("Serialization is null");

            //    List<byte[]> list = Serialization.Deserialize<List<byte[]>>(data, 0, data.Length);

            //    Type memberType = type.GetMethod("Get").ReturnType;

            //    var array = Array.CreateInstance(memberType, list.Count);

            //    for (int i = 0; i < list.Count; i++)
            //    {
            //        array.SetValue(UnpackSingleObject(memberType, list[i]), i);
            //    }

            //    return array;
            //}
            //else
            //{
            //    if (Serialization is null)
            //        throw new NullReferenceException("Serialization is null");

            //    return Serialization.Deserialize(type, data, 0, data.Length);

            //}

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
