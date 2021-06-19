using System;
using System.Collections.Generic;
using System.Text;
using ZYSocket.Interface;

namespace Netx
{
    public class MsgpkgFormat : ISerialization
    {
        public T Deserialize<T>(byte[] data, int offset, int length)
        {
            return Swifter.MessagePack.MessagePackFormatter.DeserializeObject<T>(new ArraySegment<byte>(data, offset, length));
        }

        public object Deserialize(Type type, byte[] data, int offset, int length)
        {         
            return Swifter.MessagePack.MessagePackFormatter.DeserializeObject(new ArraySegment<byte>(data, offset, length),type);
        }

        public byte[] Serialize(object? obj)
        {
            return Swifter.MessagePack.MessagePackFormatter.SerializeObject(obj);
        }
    }
}
