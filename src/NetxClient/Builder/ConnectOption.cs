namespace Netx.Client
{
    public class ConnectOption
    {
        public string Host { get; set; } = "127.0.0.1";

        public int Port { get; set; } = 1002;

        public string ServiceName { get; set; } = "";

        public string VerifyKey { get; set; } = "";

        public int BufferSize { get; set; } = 4096;

        public int MaxPackerSize { get; set; } = 128 * 1024;

        public int ConnectedTimeOut { get; set; } = 10000;

        public int RequestTimeOut { get; set; } = 20000;

    }
}
