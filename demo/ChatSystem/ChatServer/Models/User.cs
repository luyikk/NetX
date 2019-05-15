using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer.Models
{
    public class User
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string PassWord { get; set; }
        public byte OnLineStatus { get; set; }

    }
}
