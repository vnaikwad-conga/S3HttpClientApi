Update S3PerfTest/AWSConfig.cs file with AWS Access key and Secret before running the app. You can also update the bucket name and region.

public class AWSConfig
{
    public const string AccessKey = "";
    public const string Secret = "";
    public const string Region = "us-west-1";
    public static readonly BasicAWSCredentials Credentials = new BasicAWSCredentials(AccessKey, Secret);
    public const string BucketName = "s3-perf-validation";
}
