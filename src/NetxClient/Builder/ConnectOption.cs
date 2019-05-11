using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Client
{
    public class ConnectOption
    {
        public string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 1002;

        public string VerifyKey { get; set; }

        public int BufferSize { get; set; } = 4096;

        public int RequestTimeOut { get; set; } = 10000;
    
    }
}
