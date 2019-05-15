using ChatServer.Models;
using Netx.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer.Model
{
    public class UserInfo
    {
        public User UserContent { get;  }

        public AsyncToken Token { get; set; }

        public int Status { get; set; }

        public UserInfo(User user)
        {
            this.UserContent = user;
        }
    }
}
