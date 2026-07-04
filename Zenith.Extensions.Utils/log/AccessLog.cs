using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Zenith.Extensions.Utils
{
    public class AccessLog : Log
    {
        public new string Index => $"{ProductLine}-access";

        public string TraceId { get; set; }

        public int TraceDepth { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ProductLine ProductLine { get; set; }

        public string AppId { get; set; }

        public string Ip { get; set; }

        public long UserId { get; set; }

        public long ClientId { get; set; }

        public string UrlReferrer { get; set; }

        public string Method { get; set; }

        public string ApiHost { get; set; }

        public string ApiEndpoint { get; set; }

        public string QueryString { get; set; }

        public string Body { get; set; }

        private string _responseBody;
        public string ResponseBody
        {
            get
            {
                return _responseBody;
            }
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Length > 20000)
                {
                    _responseBody = value.Substring(0, 20000);
                }
                else
                {
                    _responseBody = value;
                }
            }
        }

        public int TimeElapsed { get; set; }

    }
}
