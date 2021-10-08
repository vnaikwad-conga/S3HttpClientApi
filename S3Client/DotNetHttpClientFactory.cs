#region Using Directives
using Amazon.Runtime;
using System;
using System.Net.Http;
using System.Net.Sockets;

#endregion

namespace S3Client
{
    public class DotNetHttpClientFactory : HttpClientFactory
    {
        HttpClient httpClient = null;
        public DotNetHttpClientFactory(IHttpClientFactory clientFactory)
        {
            httpClient = clientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.ConnectionClose = false;
        }

        public override HttpClient CreateHttpClient(IClientConfig clientConfig)
        {
            return httpClient;
        }
    }
}
