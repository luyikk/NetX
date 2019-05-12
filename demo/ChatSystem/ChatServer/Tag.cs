using System;
using System.Collections.Generic;
using System.Text;

namespace ChatServer
{
    public enum ActorTag
    {
        Register=1,
        CheckUserName=2,
        CheckUserNameAndPassword=3,
        GetUsers=4
    }

    public enum ServiceTag
    {
        LogOn = 1001,
        CheckLogIn=1002
    }
}
