using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using ChatServer.Model;

namespace ChatServer.AsyncControllers
{
    public class UserManager
    {
        readonly Lazy<ConcurrentDictionary<long, UserInfo>> userList;

        public ConcurrentDictionary<long, UserInfo> UserList { get => userList.Value; }

        public UserManager()
        {
            userList = new Lazy<ConcurrentDictionary<long, UserInfo>>();
        }
    }
}
