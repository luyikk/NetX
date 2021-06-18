using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.FiberStream;

namespace Netx.Service
{



    public abstract class ServiceDecodeSetter : ServiceToken
    {

        protected readonly bool is_use_ssl;
        protected X509Certificate? Certificate { get; }
        protected Func<Stream, Task<SslStream>>? SslStreamInit { get; }

        protected readonly CompressType decodeType;

        public ServiceDecodeSetter(IServiceProvider container) : base(container)
        {
            var ssloption = container.GetRequiredService<IOptions<SslOption>>().Value;
            if (ssloption.IsUse)
            {
                Certificate = ssloption.Certificate;
                SslStreamInit=ssloption.SslStreamInit;
                is_use_ssl = true;
            }

            var compresstion = container.GetRequiredService<IOptions<CompressOption>>().Value;
            decodeType = compresstion.Mode;
        }

        /// <summary>
        /// 返回FiberRw
        /// </summary>
        /// <param name="socketAsync"></param>
        /// <returns></returns>
        protected virtual async Task<IFiberRw<AsyncToken>?> GetFiberRw(ISockAsyncEventAsServer socketAsync)
        {
            if (is_use_ssl) //SSL Config
            {
                var (fiber, msg) = await GetSslFiber(socketAsync);

                if (fiber is null)
                {
                    if (msg != null)
                        Log.Error(msg);
                    return null;
                }
                else
                    return fiber;
            }
            else
            {

                return decodeType switch
                {
                    CompressType.None => await socketAsync.GetFiberRw<AsyncToken>(),
                    CompressType.gzip => await socketAsync.GetFiberRw<AsyncToken>((input, output) =>
                    {
                        var gzip_input = new GZipStream(input, CompressionMode.Decompress, true);
                        var gzip_output = new GZipStream(output, CompressionMode.Compress, true);
                        return new GetFiberRwResult(gzip_input, gzip_output); //return gzip mode
                    }),
                    CompressType.lz4 => await socketAsync.GetFiberRw<AsyncToken>((input, output) =>
                    {
                        var lz4_input = K4os.Compression.LZ4.AsyncStreams.LZ4Stream.Decode(input, leaveOpen: true);
                        var lz4_output = K4os.Compression.LZ4.AsyncStreams.LZ4Stream.Encode(output, leaveOpen: true);
                        return new GetFiberRwResult(lz4_input, lz4_output); //return lz4 mode
                    }),
                    _ => throw new NotImplementedException()
                };
            }

        }

        private async ValueTask<(IFiberRw<AsyncToken>?, string?)> GetSslFiber(ISockAsyncEventAsServer socketAsync)
        {
            if (SslStreamInit is null)
            {
                return decodeType switch
                {
                    CompressType.None => await socketAsync.GetFiberRwSSL<AsyncToken>(Certificate!),
                    CompressType.gzip => await socketAsync.GetFiberRwSSL<AsyncToken>(Certificate!, (input, output) =>
                    {
                        var gzip_input = new GZipStream(input, CompressionMode.Decompress, true);
                        var gzip_output = new GZipStream(output, CompressionMode.Compress, true);
                        return new GetFiberRwResult(gzip_input, gzip_output); //return gzip mode
                }),
                    CompressType.lz4 => await socketAsync.GetFiberRwSSL<AsyncToken>(Certificate!, (input, output) =>
                    {
                        var lz4_input = K4os.Compression.LZ4.AsyncStreams.LZ4Stream.Decode(input, leaveOpen: true);
                        var lz4_output = K4os.Compression.LZ4.AsyncStreams.LZ4Stream.Encode(output, leaveOpen: true);
                        return new GetFiberRwResult(lz4_input, lz4_output); //return lz4 mode
                }),
                    _ => throw new NotImplementedException()
                };
            }
            else
            {
                return decodeType switch
                {
                    CompressType.None => await socketAsync.GetFiberRwSSL<AsyncToken>(sslstream_init:SslStreamInit),
                    CompressType.gzip => await socketAsync.GetFiberRwSSL<AsyncToken>(sslstream_init: SslStreamInit,(input, output) =>
                    {
                        var gzip_input = new GZipStream(input, CompressionMode.Decompress, true);
                        var gzip_output = new GZipStream(output, CompressionMode.Compress, true);
                        return new GetFiberRwResult(gzip_input, gzip_output); //return gzip mode
                    }),
                    CompressType.lz4 => await socketAsync.GetFiberRwSSL<AsyncToken>(sslstream_init: SslStreamInit,(input, output) =>
                    {
                        var lz4_input = K4os.Compression.LZ4.AsyncStreams.LZ4Stream.Decode(input, leaveOpen: true);
                        var lz4_output = K4os.Compression.LZ4.AsyncStreams.LZ4Stream.Encode(output, leaveOpen: true);
                        return new GetFiberRwResult(lz4_input, lz4_output); //return lz4 mode
                    }),
                    _ => throw new NotImplementedException()
                };
            }
        }
    }
}
