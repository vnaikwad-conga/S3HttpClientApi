﻿namespace S3PerfTest
{
    public class DocumentResponseModel
    {
        public string Key { get; set; }
        public string Path { get; set; }
        public byte[] Data { get; set; }
    }
}