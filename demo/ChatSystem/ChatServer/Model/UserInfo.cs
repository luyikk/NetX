using Netx.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer.Model
{
    public class UserInfo
    {
        public Users UserContent { get;  }

        public AsyncToken Token { get; set; }

        public int Status { get; set; }

        public UserInfo(Users user)
        {
            this.UserContent = user;
        }
    }
}
