﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ChatClient
{
    public class LeavingMsg
    {
        public long Time { get; set; }
        public byte MsgType { get; set; }
        public long FromUserId { get; set; }
        public string NickName { get; set; }
        public string MessageContext { get; set; }
    }
}
