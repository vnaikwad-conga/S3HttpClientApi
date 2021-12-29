using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace S3PerfTest
{
    class Program
    {
        private static Random random = new Random();
        private static readonly AmazonS3Client s3Client = new AmazonS3Client(AWSConfig.Credentials, RegionEndpoint.GetBySystemName(AWSConfig.Region));
        private static HttpClient _httpClient = new HttpClient();

        private static readonly string objectAId = "ce420020-bb6e-41be-82df-329e1c7b8b2f";
        private static readonly string objectBId = "76dec12e-219a-4415-88b3-b1dee375a27d";
        private static readonly string objectCId = "e8336192-6353-11ec-90d6-0242ac120003";
        private static readonly string objectDId = "f5dff99a-6353-11ec-90d6-0242ac120003";
        private static readonly string objectEId = "0cf3d570-6354-11ec-90d6-0242ac120003";

        private static List<TimeDetail> sdkTimes = new List<TimeDetail>();
        private static List<TimeDetail> httpClientTimes = new List<TimeDetail>();

        static void Main(string[] args)
        {
            int delay = 3;
            if (args.Length == 0)
            {
                Console.WriteLine("Using default delay of 3 seconds");
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

            ConfigureLog4Net();
            RunS3TestsAsync(cts.Token, delay).GetAwaiter().GetResult();
            Console.WriteLine("Press ENTER to terminate the application");
            Console.ReadLine();
        }

        private static async Task RunS3TestsAsync(CancellationToken token, int delay = 0)
        {
            Console.WriteLine("Enter choice:");
            Console.WriteLine("1. Run With SDK");
            Console.WriteLine("2. Run With REST API");

            int choice = int.Parse(Console.ReadLine());
            if (choice == 1)
            {
                Console.WriteLine("Running test using AWS SDK for S3");
                await RunS3TestAsync(token, delay);
            }
            else if (choice == 2)
            {
                Console.WriteLine("Running test using AWS REST API");
                await RunS3WithHttpClientAsync(token, GetPresignedUrl(), delay);
            }
            else
            {
                Console.WriteLine("Invalid choice. Exiting");
            }
        }

        private static async Task RunS3WithHttpClientAsync(
            CancellationToken token, string presignedUrl, int delay = 0)
        {
            while (!token.IsCancellationRequested)
            {
                Console.WriteLine($"************************* Test Start **************************");
                presignedUrl = GetPresignedUrl();
                var profiler = MiniProfiler.StartNew($"S3-REST-API-Read-Stats");
                var timing = profiler.Step("Read-Object");
                var response = await _httpClient.GetAsync(presignedUrl);
                var content = await response.Content.ReadAsStringAsync();
                timing.Stop();

                profiler.Stop();

                response.EnsureSuccessStatusCode();

                if (timing.DurationMilliseconds > 50)
                {
                    httpClientTimes.Add(new TimeDetail
                    {
                        RequestId = 0,
                        LatencyMs = timing.DurationMilliseconds.ToString(),
                        Timestamp = DateTime.Now
                    });
                }

                Console.WriteLine(profiler.RenderPlainText());
                Console.WriteLine($"************************* Test End **************************\n\n");
                await Task.Delay(delay * 1000);
            }

            Console.WriteLine("Printing HTTP Client times");
            Console.WriteLine(JsonSerializer.Serialize(httpClientTimes));
        }

        private static string GetPresignedUrl()
        {
            var presignedUrl = s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
            {
                BucketName = "s3-perf-validation",
                Key = $"objects/{GetDocId()}",
                Expires = DateTime.UtcNow.AddMinutes(2),
                Verb = HttpVerb.GET,
            });

            return presignedUrl;
        }

        private static void ConfigureLog4Net()
        {
            AWSConfigs.LoggingConfig.LogTo = LoggingOptions.Console;
            AWSConfigs.LoggingConfig.LogMetricsFormat = LogMetricsFormatOption.Standard;
            AWSConfigs.LoggingConfig.LogMetrics = true;
            AWSConfigs.LoggingConfig.LogResponsesSizeLimit = 100;

            //XmlConfigurator.Configure(new FileInfo("App.config"));
        }

        private static string GetDocId()
        {
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

        private static async Task RunS3TestAsync(CancellationToken token, int delay = 0)
        {
            while (!token.IsCancellationRequested)
            {
                Console.WriteLine($"************************* Test Start **************************");
                var profiler = MiniProfiler.StartNew($"S3-Read-Client-Stats");
                var dataProvider = new S3Blob();
                var docId = GetDocId();
                var timing = profiler.Step($"Read-Object");
                await ReadObjectAsync(profiler, dataProvider, docId);
                timing.Stop();
                profiler.Stop();

                if (timing.DurationMilliseconds > 50)
                {
                    sdkTimes.Add(new TimeDetail
                    {
                        LatencyMs = timing.DurationMilliseconds.ToString(),
                        Timestamp = DateTime.Now,
                        RequestId = 0
                    });
                }

                Console.WriteLine(profiler.RenderPlainText());
                Console.WriteLine($"************************* Test End **************************\n\n");
                await Task.Delay(delay * 1000);
            }

            Console.WriteLine("Printing sdkTimes");
            Console.WriteLine(JsonSerializer.Serialize(sdkTimes));
        }

        private static async Task ReadObjectAsync(
            MiniProfiler profiler, IDataProvider dataProvider, string docId = null)
        {
            var documentResponseModel = await dataProvider.GetObjectAsync(new DocumentRequestModel
            {
                Key = docId,
                Path = "objects"
            });
        }
    }

    internal class TimeDetail
    {
        public int RequestId { get; set; }
        public string LatencyMs { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
