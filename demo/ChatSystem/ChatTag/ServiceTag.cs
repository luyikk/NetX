using System;

namespace ChatTag
{
    public enum ActorTag
    {
        Register = 1,
        CheckUserName = 2,
        CheckUserNameAndPassword = 3,
        GetUsers = 4,
        SetStatus=5
    }

    public enum ServiceTag
    {
        LogOn = 1001,
        CheckLogIn = 1002,
        GetUserList = 1003
    }
}
