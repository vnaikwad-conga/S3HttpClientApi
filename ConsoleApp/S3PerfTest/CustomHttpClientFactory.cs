using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace S3PerfTest
{
    public class CustomHttpClientFactory : HttpClientFactory
    {
        static HttpClient httpClient = null;
        static CustomHttpClientFactory()
        {
            var socketsHandler = new SocketsHttpHandler
            {
                KeepAlivePingPolicy = HttpKeepAlivePingPolicy.Always,
                KeepAlivePingDelay = TimeSpan.FromSeconds(2),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(2)
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
