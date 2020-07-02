namespace Netx
{
    /// <summary>
    /// 压缩方式
    /// </summary>
    public enum CompressType
    {
        /// <summary>
        /// 没有
        /// </summary>
        None = 0,
        /// <summary>
        /// GZIP
        /// </summary>
        gzip = 1,
        /// <summary>
        /// LZ4
        /// </summary>
        lz4 = 2
    }

    public class CompressOption
    {
        /// <summary>
        /// 压缩方式
        /// </summary>
        public CompressType Mode { get; set; } = CompressType.None;
    }
}
