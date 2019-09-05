using SpanJson;
using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.Interface;

namespace NetxSerializes
{
    public class JSONSerializes : ISerialization
    {
        public T Deserialize<T>(byte[] data, int offset, int length)
        {
            return JsonSerializer.Generic.Utf8.Deserialize<T>(new ReadOnlySpan<byte>(data, offset, length));
        }

        public object Deserialize(Type type, byte[] data, int offset, int length)
        {
            return JsonSerializer.NonGeneric.Utf8.Deserialize(new ReadOnlySpan<byte>(data, offset, length), type);
        }

        public byte[] Serialize(object obj)
        {
            var data= JsonSerializer.NonGeneric.Utf8.Serialize(obj);
            return data;
        }
    }
}
