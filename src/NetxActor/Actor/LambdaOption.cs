using System;
using System.Collections.Generic;
using System.Text;

namespace Netx.Actor
{
    public class LambdaOption
    {
        /// <summary>
        /// 每一个KEY 表示 每一个独立的Lambda 运行环境他们互不影响,默认只有一个default,要定义独立的lambda actor 运行环境,请添加新的key
        /// </summary>
        public HashSet<string> LambadKeys { get; set; } = new HashSet<string> { "default" };
    }
}
