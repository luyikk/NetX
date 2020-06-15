using System.Security.Cryptography.X509Certificates;

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
    }
}
