using System;
using System.Collections.Generic;

namespace Netx
{
    /// <summary>
    /// 返回结果
    /// </summary>
    public class Result : IResult
    {
        /// <summary>
        /// 调用唯一标识
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 返回值
        /// </summary>
        public List<byte[]>? Arguments { get; }

        /// <summary>
        /// 是否发生错误
        /// </summary>
        public bool IsError => ErrorId != 0;

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMsg { get; set; }
        /// <summary>
        /// 错误ID,如果错误有
        /// </summary>
        public int ErrorId { get; set; }

        /// <summary>
        /// 是否拥有返回值
        /// </summary>
        public bool IsHaveValue => Arguments != null && Arguments.Count > 0;

        /// <summary>
        /// 长度
        /// </summary>
        public int? Length => Arguments?.Count;

        /// <summary>
        /// 索引
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IResultValue? this[int index]
        {
            get
            {
                if (IsError)
                    throw new NetxException(ErrorMsg, ErrorId);

                if (Arguments == null)
                    return null;

                if (index < Arguments.Count)
                {
                    return new ResultValue(Arguments[index]);
                }

                return null;
            }
        }

        /// <summary>
        /// 第一个返回值
        /// </summary>
        public IResultValue? First
        {
            get
            {
                if (IsError)
                    throw new NetxException(ErrorMsg, ErrorId);

                if (Arguments == null || Arguments.Count == 0)
                    return null;

                return new ResultValue(Arguments[0]);
            }

        }

        public Result()
        {
            Arguments = new List<byte[]>();
        }

        public Result(List<byte[]> args)
        {
            if (args != null)
                Arguments = args;
            else
                Arguments = new List<byte[]>();
        }


        public Result(params object[] args)
        {
            if (args != null)
            {
                Arguments = new List<byte[]>();
                foreach (var arg in args)
                    Arguments.Add(SerializationPacker.PackSingleObject(arg));
            }
        }

        /// <summary>
        /// 将指定索引转换成指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index"></param>
        /// <returns></returns>
        public T As<T>(int index)
        {
            if (IsError)
                throw new NetxException(ErrorMsg, ErrorId);

            if (Length <= 0 || Arguments is null)
                throw new NetxException("null value", ErrorType.NotValue);

            return (T)SerializationPacker.UnpackSingleObject(typeof(T), Arguments[index]);
        }

        /// <summary>
        /// 将默认索引为0的数据转换成指定类型的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T As<T>()
        {
            if (IsError)
                throw new NetxException(ErrorMsg, ErrorId);

            if (Length <= 0 || Arguments is null)
                throw new NetxException("null value", ErrorType.NotValue);

            return (T)SerializationPacker.UnpackSingleObject(typeof(T), Arguments[0]);
        }

        /// <summary>
        /// 将指定索引转换成指定类型的对象
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="index">索引</param>
        /// <returns></returns>
        public object As(Type type, int index)
        {
            if (IsError)
                throw new NetxException(ErrorMsg, ErrorId);

            if (Length <= 0 || Arguments is null)
                throw new NetxException("null value", ErrorType.NotValue);

            if (index > Length)
                throw new NetxException("not find value index error", ErrorType.IndexError);

            return SerializationPacker.UnpackSingleObject(type, Arguments[index]);
        }
    }
}
