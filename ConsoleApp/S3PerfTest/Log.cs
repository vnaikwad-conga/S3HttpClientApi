using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3PerfTest
{
    internal class Log
    {
        public DateTime RequestStart { get; set; }
        public DateTime RequestStop { get; set; }
        public double Request => (RequestStop - RequestStart).TotalMilliseconds;

        public DateTime? ResolutionStart { get; set; }
        public DateTime? ResolutionStop { get; set; }
        public double? Resolution => (ResolutionStop - ResolutionStart)?.TotalMilliseconds;

        public DateTime? ConnectStart { get; set; }
        public DateTime? ConnectStop { get; set; }
        public double? Connect => (ConnectStop - ConnectStart)?.TotalMilliseconds;


        public DateTime? HandshakeStart { get; set; }
        public DateTime? HandshakeStop { get; set; }
        public double? Handshake => (HandshakeStop - HandshakeStart)?.TotalMilliseconds;

        public DateTime RequestHeadersStart { get; set; }
        public DateTime RequestHeadersStop { get; set; }
        public double RequestHeaders => (RequestHeadersStop - RequestHeadersStart).TotalMilliseconds;

        public DateTime ResponseHeadersStart { get; set; }
        public DateTime ResponseHeadersStop { get; set; }
        public double ResponseHeaders => (ResponseHeadersStop - ResponseHeadersStart).TotalMilliseconds;


        public DateTime SdkRequestStart { get; set; }
        public DateTime SdkRequestEnd { get; set; }
        public double SdkRequest => (SdkRequestEnd - SdkRequestStart).TotalMilliseconds;


        public DateTime ResponseReadStart { get; set; }
        public DateTime ResponseReadEnd { get; set; }
        public double ResponseRead => (ResponseReadEnd - ResponseReadStart).TotalMilliseconds;

        public List<string> Texts = new List<string>();

        public override string ToString()
        {
            var now = DateTime.UtcNow;
            return $"{now.ToString("yyyy-MM-dd hh:mm:ss:fffffff tt")},{Request},{Resolution},{Connect},{Handshake},{RequestHeaders},{ResponseHeaders},{SdkRequest},{ResponseRead}";
            //return $"{string.Join(Environment.NewLine, Texts)}{Environment.NewLine}{now.ToString("yyyy-MM-dd hh:mm:ss tt")},{Request},{Resolution},{Connect},{Handshake},{RequestHeaders},{ResponseHeaders},{SdkRequest},{ResponseRead}";
        }

        public static string Header = "Time,Request,Resolution,Connect,Handshake,RequestHeaders,ResponseHeaders,SdkRequest,ResponseRead";
    }
}
