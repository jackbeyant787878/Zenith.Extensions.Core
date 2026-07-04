namespace Zenith.Extensions.Elasticsearch
{
    public class SimpleErrorLog : Log
    {
        public SimpleException Ex { get; set; }
    }

    public class SimpleException
    {
        public virtual string StackTrace { get; set; }

        public virtual string Source { get; set; }

        public virtual string Message { get; set; }

        public SimpleException InnerException { get; set; }
    }
}
