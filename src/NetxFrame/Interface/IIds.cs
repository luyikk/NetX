using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Interface
{
    /// <summary>
    /// Id生成接口
    /// </summary>
    public interface IIds
    {
        /// <summary>
        /// 生成一个Id
        /// </summary>
        long MakeId { get; }
    }
}
