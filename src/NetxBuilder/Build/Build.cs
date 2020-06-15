using System;

namespace Netx
{
    /// <summary>
    /// 用于标记需要编译的接口
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class Build : Attribute
    {

    }
}
