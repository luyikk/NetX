using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer.Model
{
    public class Message
    {
        public long MessageId { get; set; }

        public long Time { get; set; }

        public byte MsgType { get; set; }

        public long FromUserId { get; set; }

        public long TargetUserId { get; set; }

        public string MessageContext { get; set; }

        public bool IsSend { get; set; }
    }
}
