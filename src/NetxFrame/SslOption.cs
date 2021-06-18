using System;
using System.IO;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Netx
{
    public class SslOption
    {
        /// <summary>
        /// 是否使用SSL
        /// </summary>
        public bool IsUse { get; set; } = false;

        /// <summary>
        /// 证书
        /// </summary>
        public X509Certificate? Certificate { get; set; } = null;

        /// <summary>
        /// domain
        /// </summary>
        public string DoMain { get; set; } = "localhost";

        /// <summary>
        /// 自定义ssl 处理
        /// </summary>
        public Func<Stream, Task<SslStream>>? SslStreamInit { get; set; } = null;
    }
}
