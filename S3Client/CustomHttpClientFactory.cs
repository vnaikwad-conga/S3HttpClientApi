#region Using Directives
using Amazon.Runtime;
using System;
using System.Net.Http;
using System.Net.Sockets;

#endregion

namespace S3Client
{
    public class CustomHttpClientFactory : HttpClientFactory
    {
        static HttpClient httpClient = null;
        static CustomHttpClientFactory()
        {
            var socketsHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(20),
                //KeepAlivePingDelay = TimeSpan.FromSeconds(20),
                //KeepAlivePingTimeout = TimeSpan.FromSeconds(20),
                //KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
                MaxConnectionsPerServer = int.MaxValue,
                EnableMultipleHttp2Connections = true,
                ConnectCallback = async (context, token) =>
                {
                    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
                    await socket.ConnectAsync(context.DnsEndPoint, token).ConfigureAwait(false);
                    return new NetworkStream(socket, ownsSocket: true);
                }
            };
            httpClient = new HttpClient(socketsHandler);
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
        }
        public override HttpClient CreateHttpClient(IClientConfig clientConfig)
        {
            return httpClient;
        }
    }
}
