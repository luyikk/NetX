using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.Interface;
using Newtonsoft.Json;

namespace Netx
{
    public class JsonFormat : ISerialization
    {
        public T Deserialize<T>(byte[] data, int offset, int length)
        {
            var json = Encoding.UTF8.GetString(data, offset, length);
            var res= JsonConvert.DeserializeObject<T>(json);
            if (res is null)
                throw new InvalidOperationException($"deserialez error:{json} to type:{typeof(T).FullName} error!");
            return res;          
        }

        public object Deserialize(Type type, byte[] data, int offset, int length)
        {
            var json = Encoding.UTF8.GetString(data, offset, length);
            var res = JsonConvert.DeserializeObject(json, type);
            if (res is null)
                throw new InvalidOperationException($"deserialez error:{json} to type:{type.FullName} error!");
            return res;
        }

        public byte[] Serialize(object? obj)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
        }
    }
}
