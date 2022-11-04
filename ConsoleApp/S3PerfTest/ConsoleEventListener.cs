using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace S3PerfTest
{
    internal sealed class ConsoleEventListener : EventListener
    {
        public ConsoleEventListener(Log log)
        {
            Log = log;

            foreach (EventSource source in EventSource.GetSources())
            {
                EnableEvents(source, EventLevel.LogAlways);
            }
        }

        public Log Log { get; }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            base.OnEventSourceCreated(eventSource);
            EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var key = $"{eventData.EventName}";
            var time = DateTime.UtcNow;
            switch (key)
            {
                case "RequestStart":
                    Log.RequestStart = time;
                    break;
                case "RequestStop":
                    Log.RequestStop = time;
                    break;
                case "ResolutionStart":
                    Log.ResolutionStart = time;
                    break;
                case "ResolutionStop":
                    Log.ResolutionStop = time;
                    break;
                case "ConnectStart":
                    Log.ConnectStart = time;
                    break;
                case "ConnectStop":
                    Log.ConnectStop = time;
                    break;
                case "HandshakeStart":
                    Log.HandshakeStart = time;
                    break;
                case "HandshakeStop":
                    Log.HandshakeStop = time;
                    break;
                case "RequestHeadersStart":
                    Log.RequestHeadersStart = time;
                    break;
                case "RequestHeadersStop":
                    Log.RequestHeadersStop = time;
                    break;
                case "ResponseHeadersStart":
                    Log.ResponseHeadersStart = time;
                    break;
                case "ResponseHeadersStop":
                    Log.ResponseHeadersStop = time;
                    break;
                default:
                    break;
            }

            var now = DateTime.UtcNow;
            string text = $"{now.ToString("yyyy-MM-dd hh:mm:ss tt")}[{eventData.EventSource.Name}-{eventData.EventName}]{(eventData.Payload != null ? $" ({string.Join(", ", eventData.Payload)})." : "")}";
            if (eventData.EventSource.Name.Contains("System.Net.Http"))
            {
                Log?.Texts.Add(text);
            }
        }
    }
}
