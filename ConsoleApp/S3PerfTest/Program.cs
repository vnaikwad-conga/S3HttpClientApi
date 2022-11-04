using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Azure.Storage.Blobs;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace S3PerfTest
{
    class Program
    {
        private static int Index = 1;
        private static Random random = new Random();

        private static readonly string objectAId = "ce420020-bb6e-41be-82df-329e1c7b8b2f";
        private static readonly string objectBId = "76dec12e-219a-4415-88b3-b1dee375a27d";
        private static readonly string objectCId = "e8336192-6353-11ec-90d6-0242ac120003";
        private static readonly string objectDId = "f5dff99a-6353-11ec-90d6-0242ac120003";
        private static readonly string objectEId = "0cf3d570-6354-11ec-90d6-0242ac120003";

        private static string objectAURL;
        private static string objectBURL;
        private static string objectCURL;
        private static string objectDURL;
        private static string objectEURL;

        private static int Iteration = 1;

        static async Task Main(string[] args)
        {
            // NOTE: Uncomment this to run PUT related tests
            //var numDocs = int.Parse(args[0]);

            //if (args[1] == "aws")
            //{
            //    await UploadS3Async(numDocs);
            //}
            //else if (args[1] == "azure")
            //{
            //    await UploadAzureBlobs(numDocs);
            //}
            //else
            //{
            //    Console.WriteLine("Args did not match");
            //}

            //return;

            int delay = 0;
            if (args.Length == 0)
            {
                Console.WriteLine($"Using default delay of {delay} seconds");
            }
            else
            {
                delay = int.Parse(args[0]);
            }

            Console.WriteLine($"This test will run with {delay} seconds delay");
            var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            Console.WriteLine("Enter Choice");
            Console.WriteLine("1. Run with AWS S3 SDK");
            Console.WriteLine("2. Run with REST API");
            Console.WriteLine("3. Run against Azure Blob");
            int choice = int.Parse(Console.ReadLine());
            switch (choice)
            {
                case 1:
                    await RunS3PerfTestAsync(cts.Token, delay);
                    break;

                case 2:
                    await RunS3WithHttpClientAsync(cts.Token, delay);
                    break;

                case 3:
                    await TestAzureBlobStorageAsync(cts.Token, delay);
                    break;

                default:
                    throw new InvalidDataException("Choice");
            }

            Console.WriteLine("Press ENTER to terminate the application");
            Console.ReadLine();
        }

        private static async Task RunS3PerfTestAsync(CancellationToken cancellationToken, int delay)
        {
            var amazonS3Config = new AmazonS3Config()
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(AWSConfig.Region),
                HttpClientFactory = new CustomHttpClientFactory()
            };

            var s3Client = new AmazonS3Client(AWSConfig.AccessKey, AWSConfig.Secret, amazonS3Config);

            var path = Path.Combine("Results", $"{DateTime.UtcNow.ToString("yyyy-MM-dd-THH-mm-ss")}.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await File.AppendAllLinesAsync(path, new[] { Log.Header });

            while (!cancellationToken.IsCancellationRequested)
            {
                var log = new Log();
                using (var listener = new ConsoleEventListener(log))
                {
                    log.SdkRequestStart = DateTime.UtcNow;

                    var obj = await s3Client.GetObjectAsync(new GetObjectRequest
                    {
                        BucketName = AWSConfig.BucketName,
                        Key = $"objects/{GetDocId()}"
                    });

                    log.SdkRequestEnd = DateTime.UtcNow;
                    log.ResponseReadStart = DateTime.UtcNow;

                    using (var reader = new StreamReader(obj.ResponseStream))
                    {
                        var time = DateTime.UtcNow;
                        var result = reader.ReadToEnd();
                        var elapsed = DateTime.UtcNow - time;
                    }
                    log.ResponseReadEnd = DateTime.UtcNow;
                    //logs.Add(log);
                }

                await File.AppendAllLinesAsync(path, new[] { log.ToString() });

                await Task.Delay(delay * 1000);
            }
        }

        private static async Task RunS3WithHttpClientAsync(
            CancellationToken token, int delay = 0)
        {
            var amazonS3Config = new AmazonS3Config()
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(AWSConfig.Region),
                HttpClientFactory = new CustomHttpClientFactory()
            };

            var s3Client = new AmazonS3Client(AWSConfig.AccessKey, AWSConfig.Secret, amazonS3Config);

            objectAURL = GetPresignedUrl(s3Client, objectAId);
            objectBURL = GetPresignedUrl(s3Client, objectBId);
            objectCURL = GetPresignedUrl(s3Client, objectCId);
            objectDURL = GetPresignedUrl(s3Client, objectEId);
            objectEURL = GetPresignedUrl(s3Client, objectEId);

            var path = Path.Combine("RestAPIResults", $"{DateTime.UtcNow.ToString("yyyy-MM-dd-THH-mm-ss")}.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await File.AppendAllLinesAsync(path, new[] { Log.Header });

            var httpClient = new HttpClient();

            while (!token.IsCancellationRequested)
            {
                var log = new Log();
                using (var listener = new ConsoleEventListener(log))
                {
                    var presignedUrl = GetPresignedUrlByDocId();

                    log.SdkRequestStart = DateTime.UtcNow;
                    var response = await httpClient.GetAsync(presignedUrl);
                    log.SdkRequestEnd = DateTime.UtcNow;

                    log.ResponseReadStart = DateTime.UtcNow;
                    var content = await response.Content.ReadAsStringAsync();
                    response.EnsureSuccessStatusCode();

                    log.ResponseReadEnd = DateTime.UtcNow;
                }

                await File.AppendAllLinesAsync(path, new[] { log.ToString() });
                await Task.Delay(delay * 1000);
            }
        }

        private static string GetPresignedUrl(AmazonS3Client s3Client, string docId)
        {
            var presignedUrl = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = "s3-perf-validation",
                Key = $"objects/{docId}",
                Expires = DateTime.UtcNow.AddHours(2),
                Verb = HttpVerb.GET,
            });

            return presignedUrl;
        }

        private static string GetPresignedUrlByDocId()
        {
            switch (random.Next(1, 6))
            {
                case 1:
                    return objectAURL;
                case 2:
                    return objectBURL;
                case 3:
                    return objectCURL;
                case 4:
                    return objectDURL;
                case 5:
                    return objectEURL;
                default:
                    return objectAURL;
            }
        }

        private static string GetDocId()
        {
            var docId = "object" + Iteration;
            Iteration++;
            return docId;
            switch (random.Next(1, 6))
            {
                case 1:
                    return objectAId;
                case 2:
                    return objectBId;
                case 3:
                    return objectCId;
                case 4:
                    return objectDId;
                case 5:
                    return objectEId;
                default:
                    return objectAId;
            }
        }

        private static async Task TestAzureBlobStorageAsync(
            CancellationToken token, int delay = 0)
        {
            var path = Path.Combine("AzureBlobResults", $"{DateTime.UtcNow.ToString("yyyy-MM-dd-THH-mm-ss")}.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            await File.AppendAllLinesAsync(path, new[] { Log.Header });

            while (!token.IsCancellationRequested)
            {
                var log = new Log();
                string data;
                using (var listener = new ConsoleEventListener(log))
                {
                    log.SdkRequestStart = DateTime.UtcNow;
                    var docId = GetDocId();
                    if (Iteration > 101)
                    {
                        break;
                    }

                    var blobClient = containerClient.GetBlobClient(docId);

                    var response = await blobClient.DownloadStreamingAsync();
                    log.SdkRequestEnd = DateTime.UtcNow;

                    log.ResponseReadStart = DateTime.UtcNow;
                    
                    using (var reader = new StreamReader(response.Value.Content))
                    {
                        data = reader.ReadToEnd();
                    }
                    log.ResponseReadEnd = DateTime.UtcNow;
                }

                Console.WriteLine(data);
                await File.AppendAllLinesAsync(path, new[] { log.ToString() });
                await Task.Delay(delay * 1000);
            }
        }

        private static BlobContainerClient containerClient = new BlobContainerClient(@"[Enter valid connection string here]", "objects");

        private static BlobClient blobA = containerClient.GetBlobClient(objectAId);
        private static BlobClient blobB = containerClient.GetBlobClient(objectBId);
        private static BlobClient blobC = containerClient.GetBlobClient(objectCId);
        private static BlobClient blobD = containerClient.GetBlobClient(objectDId);
        private static BlobClient blobE = containerClient.GetBlobClient(objectEId);

        private static BlobClient GetBlobClient()
        {
            switch (random.Next(1, 6))
            {
                case 1:
                    return blobA;
                case 2:
                    return blobB;
                case 3:
                    return blobC;
                case 4:
                    return blobD;
                case 5:
                    return blobE;
                default:
                    return blobA;
            }
        }

        private static async Task UploadAzureBlobs(int numDocs)
        {
            var content = File.ReadAllText(@"Data/objectA.json");
            for (int i = 1; i <= numDocs; i++)
            {
                BlobClient blob = containerClient.GetBlobClient("object" + i);
                var formattedContent = content.Replace("ToReplaceId", "object" + i);
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(formattedContent)))
                {                    
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    var response = await blob.UploadAsync(ms);
                    stopwatch.Stop();
                    Console.WriteLine($"UploadAzureBlobAsync time: {stopwatch.ElapsedMilliseconds}ms");
                }
            }
        }

        private static async Task UploadS3Async(int numDocs)
        {
            var amazonS3Config = new AmazonS3Config()
            {
                RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(AWSConfig.Region),
            };

            var s3Client = new AmazonS3Client(AWSConfig.AccessKey, AWSConfig.Secret, amazonS3Config);
            var content = File.ReadAllText(@"Data/objectA.json");

            for (int i = 1; i <= numDocs; i++)
            {
                var formattedContent = content.Replace("ToReplaceId", "object" + i);
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(formattedContent)))
                {
                    var putObjectRequest = new PutObjectRequest
                    {
                        InputStream = ms,
                        Key = $"objects/object{i}",
                        BucketName = "s3-put-perf-test"
                    };

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    var response = await s3Client.PutObjectAsync(putObjectRequest);
                    stopwatch.Stop();
                    Console.WriteLine($"PutObjectAsync time: {stopwatch.ElapsedMilliseconds}ms");
                }
            }
        }
    }
}
