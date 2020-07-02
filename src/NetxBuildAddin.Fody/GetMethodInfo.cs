using System;
using System.Reflection;

public static class GetMethodInfo
{
    public static MethodInfo GetTypeofHandler()
    {
        System.Reflection.MethodInfo Type_GetTypeFromHandle = typeof(Type).GetRuntimeMethod("GetTypeFromHandle", new Type[] { typeof(RuntimeTypeHandle) });

        return Type_GetTypeFromHandle;
    }
}

