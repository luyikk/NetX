using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ZYSocket;
using ZYSocket.FiberStream;

namespace Netx.Client
{
    public abstract class NetxDecodeSetter : NetxAnalysis
    {
        protected readonly bool is_use_ssl;
        protected X509Certificate? Certificate { get; }

        protected readonly CompressType decodeType;

        public NetxDecodeSetter(IServiceProvider container)
        : base(container)
        {
            var ssloption = container.GetRequiredService<IOptions<SslOption>>().Value;
            if (ssloption.IsUse)
            {
                Certificate = ssloption.Certificate;
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
        protected virtual async Task<IFiberRw?> GetFiberRw(ISockAsyncEventAsClient socketAsync)
        {
            if (is_use_ssl) //SSL Config
            {             
                var result = decodeType switch
                {
                    CompressType.None => await socketAsync.GetFiberRwSSL(Certificate!),
                    CompressType.gzip => await socketAsync.GetFiberRwSSL(Certificate!,init:(input, output) =>
                    {
                        var gzip_input = new GZipStream(input, CompressionMode.Decompress, true);
                        var gzip_output = new GZipStream(output, CompressionMode.Compress, true);
                        return new GetFiberRwResult(gzip_input, gzip_output); //return gzip mode
                    }),
                    CompressType.lz4 => await socketAsync.GetFiberRwSSL(Certificate!, init: (input, output) =>
                    {
                        var lz4_input = K4os.Compression.LZ4.AsyncStreams.LZ4Stream.Decode(input, leaveOpen: true);
                        var lz4_output = K4os.Compression.LZ4.AsyncStreams.LZ4Stream.Encode(output, leaveOpen: true);
                        return new GetFiberRwResult(lz4_input, lz4_output); //return lz4 mode
                    }),
                    _ => throw new NotImplementedException()
                };

                if (result.IsError)
                {
                    if (result.ErrMsg != null)
                        Log.Error(result.ErrMsg);
                    return null;
                }
                else
                    return result.FiberRw;
            }
            else
            {
                return decodeType switch
                {
                    CompressType.None => await socketAsync.GetFiberRw(),
                    CompressType.gzip => await socketAsync.GetFiberRw((input, output) =>
                    {
                        var gzip_input = new GZipStream(input, CompressionMode.Decompress, true);
                        var gzip_output = new GZipStream(output, CompressionMode.Compress, true);
                        return new GetFiberRwResult(gzip_input, gzip_output); //return gzip mode
                    }),
                    CompressType.lz4 => await socketAsync.GetFiberRw((input, output) =>
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
