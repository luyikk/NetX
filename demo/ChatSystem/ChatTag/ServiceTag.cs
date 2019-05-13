using System;

namespace ChatTag
{
    public enum ActorTag
    {
        Register = 1,
        CheckUserName = 2,
        CheckUserNameAndPassword = 3,
        GetUsers = 4,
        SetStatus=5,
        SaveMsg=6,
        LeavingMessage=7
    }

    public enum ServiceTag
    {
        LogOn = 1001,
        CheckLogIn = 1002,
        GetUserList = 1003,
        Say=1004,
        GetLeaving=1005
    }
}
