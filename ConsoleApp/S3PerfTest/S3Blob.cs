#region Using Directives
using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
#endregion

namespace S3PerfTest
{
    public class S3Blob : IDataProvider
    {
        private readonly IAmazonS3 s3Client;
        private string bucketName = AWSConfig.BucketName;
        private static CustomHttpClientFactory customHttpClientFactory = new CustomHttpClientFactory();

        public S3Blob()
        {
            //Case-1 Without HttpFactory
            var amazonS3Config = new AmazonS3Config()
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(AWSConfig.Region),
                MaxErrorRetry = 3,
                Timeout = TimeSpan.FromSeconds(60),
                HttpClientFactory = customHttpClientFactory
            };

            s3Client = new AmazonS3Client(AWSConfig.AccessKey, AWSConfig.Secret, amazonS3Config);
        }

        public async Task<List<DocumentResponseModel>> GetObjectAsync(params DocumentRequestModel[] documentRequests)
        {
            var streamReaderMap = new Dictionary<GetObjectResponse, MemoryStream>();

            //Created List of Task for Content Reading
            var objectContentTasks = new List<Task>();

            //Created Dictionary  so that we can return data in same order
            var documentResponsesMap = documentRequests.ToDictionary(k => k.Key, v => new DocumentResponseModel() { Key = v.Key });

            //Create GetObjectAsync Task
            var getObjectResponseTasks = documentRequests.Select(documentRequest => s3Client.GetObjectAsync(
                new GetObjectRequest()
                {
                    BucketName = bucketName,
                    Key = $"{documentRequest.Path}/{documentRequest.Key}",
                }))
                .ToList();

            try
            {
                //Run till any task pending
                while (getObjectResponseTasks.Any())
                {
                    Task<GetObjectResponse> completedGetObjectResponseTask = await Task.WhenAny(getObjectResponseTasks);
                    getObjectResponseTasks.Remove(completedGetObjectResponseTask);
                    var objResponse = await completedGetObjectResponseTask;
                    var ms = new MemoryStream();
                    streamReaderMap.Add(objResponse, ms);

                    //Run Stream reading task
                    objectContentTasks.Add(objResponse.ResponseStream.CopyToAsync(ms));
                }
            }
            catch (Exception ex)
            {                
                throw;
            }
            finally
            {
                //Wait till All Reading Task Complete
                await Task.WhenAll(objectContentTasks);

                //Copy byte array data to response map and close memory stream
                foreach (var item in streamReaderMap)
                {
                    var objectResponse = item.Key;
                    var id = objectResponse.Key.Substring(objectResponse.Key.LastIndexOf("/") + 1);
                    documentResponsesMap[id].Data = item.Value.ToArray();
                    item.Value.Close();
                }
            }

            return documentResponsesMap.Values.ToList();
        }
    }
}
