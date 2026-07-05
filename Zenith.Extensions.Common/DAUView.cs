namespace Zenith.Extensions.Common
{
    public class DAUView
    {
        public int Index { get; set; }

        public long UserId { get; set; }

        public DateTime Date { get; set; }

        public string FormatedDate { get { return Date.ToString("MM/dd/yyyy"); } }

        public string ClientName { get; set; }

        public long ClientId { get; set; }

        public string Username { get; set; }

        public int TotalMinutes { get; set; }

        public int LoginCount { get; set; }

        public int AvgMinutesPerLogin
        {
            get
            {
                return TotalMinutes / LoginCount;
            }
        }

        public string SalesLead { get; set; }
    }
}
