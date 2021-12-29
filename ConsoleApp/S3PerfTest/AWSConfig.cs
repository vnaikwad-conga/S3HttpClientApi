using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3PerfTest
{
    public class AWSConfig
    {
        public const string AccessKey = "";
        public const string Secret = "";
        public const string Region = "us-west-1";
        public static readonly BasicAWSCredentials Credentials = new BasicAWSCredentials(AccessKey, Secret);
        public const string BucketName = "s3-perf-validation";
    }
}
