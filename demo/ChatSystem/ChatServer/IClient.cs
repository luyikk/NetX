using System;
using System.Collections.Generic;
using System.Text;
using ChatServer.Model;
using ChatTag;
using Netx;

namespace ChatServer
{
    [Build]
    public interface IClient
    {
        [TAG(ClientTag.UpdateStatus)]
        void SetUserStats(long userid, byte status);

        [TAG(ClientTag.Message)]
        void SayMessage(long fromuserId, string fromusername, byte MsgType, string msg,long time);

        [TAG(ClientTag.NeedLogOn)]
        void NeedLogOn();

        [TAG(ClientTag.UserAdd)]
        void UserAdd(Users newuser);
    }
}
