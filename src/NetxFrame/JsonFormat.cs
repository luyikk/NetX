using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.Interface;

namespace Netx
{
    public class JsonFormat : ISerialization
    {
        public T Deserialize<T>(byte[] data, int offset, int length)
        {         
            var res = Swifter.Json.JsonFormatter.DeserializeObject<T>(new ArraySegment<byte>(data, offset, length), Encoding.UTF8);
            if (res is null)
                throw new InvalidOperationException($"deserialez to type:{typeof(T).FullName} error!");
            return res;          
        }

        public object Deserialize(Type type, byte[] data, int offset, int length)
        {
            var res = Swifter.Json.JsonFormatter.DeserializeObject(new ArraySegment<byte>(data, offset, length), Encoding.UTF8,type);           
            if (res is null)
                throw new InvalidOperationException($"deserialez to type:{type.FullName} error!");
            return res;
        }

        public byte[] Serialize(object? obj)
        {
            return Swifter.Json.JsonFormatter.SerializeObject(obj,Encoding.UTF8);
        }
    }
}
