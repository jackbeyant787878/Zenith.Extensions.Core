using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Zenith.Extensions.Utils
{
    public class ApiCallLog : Log
    {
        public new string Index => $"{ProductLine}-api-sdk-call";

        public long UserId { get; set; }

        public string TraceId { get; set; }

        public string SyncSource { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ProductLine ProductLine { get; set; }

        public string AppId { get; set; }

        public string ProfileId { get; set; }

        private string _actionName;
        public string ActionName
        {
            get
            {
                if (string.IsNullOrEmpty(_actionName))
                {
                    return ApiEndpoint;
                }
                else
                {
                    return _actionName;
                }
            }
            set { _actionName = value; }
        }

        public string ApiHost { get; set; }

        public string ApiEndpoint { get; set; }

        public string QueryString { get; set; }

        public dynamic Body { get; set; }

        public string HttpMethod { get; set; }

        public int ResponseCode { get; set; }

        public dynamic ResponseHeader { get; set; }

        public dynamic ResponseBody { get; set; }

        public int TimeElapsed { get; set; }

        public string Exception { get; set; }

        public int TotalCount { get; set; }

        public int SuccessCount { get; set; }
    }

    public enum ProductLine
    {
        PayPal,
        Alipay,
        WeChat,
        GooglePay,
        VISA
    }
}
