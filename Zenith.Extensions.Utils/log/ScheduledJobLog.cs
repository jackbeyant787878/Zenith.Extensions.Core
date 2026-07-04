using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace Zenith.Extensions.Utils
{
    public class ScheduledJobLog : Log
    {
        public ScheduledJobLog()
        {
            DebugInfo = new List<string>();
        }

        public string TraceId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ProductLine ProductLine { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ScheduledJob ScheduledJob { get; set; }

        public string Scope { get; set; }

        public bool IsSuccess { get; set; }

        public int TimeElapsed { get; set; }

        public string Exception { get; set; }

        public List<string> DebugInfo { get; set; }

        public string ExecutionID { get; set; }

        public int AffectedCount { get; set; }

        public void AppendDebugInfo(string message)
        {
            var dt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var msg = $"{dt}:{message}";
            DebugInfo.Add(msg);
        }

        public new string Index => $"{ProductLine}-scheduled-job";
    }


    public enum ScheduledJob
    {
        Daypart,
        Rule,
        AI,
        Report
    }
}
