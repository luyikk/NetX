using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.Interface;
using MessagePack;

namespace Netx
{
    public class MsgPackFormat : ISerialization
    {
        public T Deserialize<T>(byte[] data, int offset, int length)
        {
            return MessagePackSerializer.Deserialize<T>(new ReadOnlyMemory<byte>(data, offset, length));
        }

        public object Deserialize(Type type, byte[] data, int offset, int length)
        {
            return MessagePackSerializer.Deserialize(type,new ReadOnlyMemory<byte>(data, offset, length));
        }

        public byte[] Serialize(object obj)
        {
            return MessagePackSerializer.Serialize(obj.GetType(), obj);
        }
    }
}
