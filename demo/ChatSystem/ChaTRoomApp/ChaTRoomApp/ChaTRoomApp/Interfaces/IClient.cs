using System;
using System.Collections.Generic;
using System.Text;
using ChaTRoomApp.Models;
using Netx;

namespace Interfaces
{

    [Build]
    public interface ISay
    {
        [TAG(1002)]
        void SayMessage(long fromuserId, string fromusername, byte MsgType, string msg, long time);
    }


    [Build]
    public interface IClient:ISay
    {
        [TAG(1001)]
        void SetUserStats(long userid, byte status);

             [TAG(1003)]
        void NeedLogOn();

        [TAG(1004)]
        void UserAdd(User newuser);
    }
}
