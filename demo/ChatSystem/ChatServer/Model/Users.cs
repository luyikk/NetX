using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer.Model
{
    public class Users
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public string PassWord { get; set; }
        public byte OnLineStatus { get; set; }

    }
}
