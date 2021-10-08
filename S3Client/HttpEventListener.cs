#region Using Directives
using System;
using System.Diagnostics.Tracing;
using System.Text;
#endregion

namespace S3Client
{    
    internal sealed class HttpEventListener : EventListener
    {
        // Constant necessary for attaching ActivityId to the events.
        public const EventKeywords TasksFlowActivityIds = (EventKeywords)0x80;

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            // List of event source names provided by networking in .NET 5.
            if (eventSource.Name == "System.Net.Http" ||
                eventSource.Name == "System.Net.Sockets" ||
                eventSource.Name == "System.Net.Security" ||
                eventSource.Name == "System.Net.NameResolution")
            {
                EnableEvents(eventSource, EventLevel.LogAlways);
            }
            // Turn on ActivityId.
            else if (eventSource.Name == "System.Threading.Tasks.TplEventSource")
            {
                // Attach ActivityId to the events.
                EnableEvents(eventSource, EventLevel.LogAlways, TasksFlowActivityIds);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            var sb = new StringBuilder().Append($"{eventData.TimeStamp:HH:mm:ss.fffffff} {eventData.EventSource.Name}.{eventData.EventName}(");
            for (int i = 0; i < eventData.Payload?.Count; i++)
            {
                sb.Append(eventData.PayloadNames?[i]).Append(": ").Append(eventData.Payload[i]);
                if (i < eventData.Payload?.Count - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append(")");            
            Console.WriteLine(sb.ToString());
            Console.WriteLine($"{eventData.TimeStamp:HH:mm:ss.fffffff}-{eventData.EventName}");
        }
    }
}
