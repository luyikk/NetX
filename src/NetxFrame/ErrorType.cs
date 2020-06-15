namespace Netx
{
    /// <summary>
    /// 错误类型
    /// </summary>
    public enum ErrorType
    {
        None = 0,
        Other = -1,
        NotValue = -101,
        IndexError = -102,
        RemoveAsyncResultBack = -103,
        FodyInstallErr = -104,
        ReturnTypeErr = -105,
        Notconnect = -106,
        NotReadCmd = -107,
        NotCmd = -108,
        ArgLenErr = -109,
        CallErr = -110,
        ConstructorsErr = -111,
        CreateInstanceErr = -112,
        NotRunType = -113,
        RegisterCmdErr = -114,
        ConnectErr = -115,
        ReadErr = -116,
        ActorErr = -117,
        ReturnModeErr = -118,
        ActorQueueMaxErr = -119,
        TimeOut = -120,
        PermissionDenied = -121
    }
}
