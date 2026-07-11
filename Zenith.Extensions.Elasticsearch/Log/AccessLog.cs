namespace Zenith.Extensions.Elasticsearch
{
    public class AccessLog : Log
    {
        public string Ip { get; set; }
        public long UserId { get; set; }

        public string Method { get; set; }
        public string Path { get; set; }

        public string QueryString { get; set; }

        public object Body { get; set; }

        public string UrlReferrer { get; set; }

        public long TimeElapsed { get; set; } 
    }
}
