using MailKit.Net.Smtp;
using MimeKit;
namespace Zenith.Extensions.Common
{
    public class CNEmailHelper
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _host;
        private readonly int _port;

        public CNEmailHelper()
        {
            _username = ConfigurationHelper.GetValue("EmailCN:Username");
            _password = ConfigurationHelper.GetValue("EmailCN:Password");
            _host = ConfigurationHelper.GetValue("EmailCN:Host");
            _port = Convert.ToInt32(ConfigurationHelper.GetValue("EmailCN:Port"));
        }

        public void Send(string receiveAdr, string subject, string htmlBody)
        {
            Send(new string[] { receiveAdr }, subject, htmlBody);
        }

        public void Send(string receiveAdr, string subject, string htmlBody, string bcc)
        {
            Send(new string[] { receiveAdr }, subject, htmlBody, new string[] { bcc });
        }

        public void Send(IEnumerable<string> receiveAdrs, string subject, string htmlBody,
            IEnumerable<string> bcc = null, IEnumerable<string> attachmentsFilePath = null)
        {
            SendAsync(receiveAdrs, subject, htmlBody, bcc, attachmentsFilePath).Wait();
        }

        public async Task SendAsync(string receiveAdr, string subject, string htmlBody)
        {
            await SendAsync(new string[] { receiveAdr }, subject, htmlBody);
        }

        public async Task SendAsync(string receiveAdr, string subject, string htmlBody, string bcc)
        {
            await SendAsync(new string[] { receiveAdr }, subject, htmlBody, new string[] { bcc });
        }

        /// <summary>
        /// i am the only function who actually do the work  -_-!
        /// </summary>
        /// <param name="receiveAdrs"></param>
        /// <param name="subject"></param>
        /// <param name="htmlBody"></param>
        /// <param name="bccs"></param>
        /// <param name="attachmentsFilePath"></param>
        /// <returns></returns>
        public async Task SendAsync(IEnumerable<string> receiveAdrs, string subject, string htmlBody,
            IEnumerable<string> bccs = null, IEnumerable<string> attachmentsFilePath = null)
        {
            try
            {
                var mail = new MimeMessage();
                mail.From.Add(new MailboxAddress("Pacvue", _username));

                foreach (string adr in receiveAdrs)
                {
                    mail.To.Add(new MailboxAddress("To", adr));
                }
                if (bccs != null)
                {
                    foreach (string adr in bccs)
                    {
                        mail.Bcc.Add(new MailboxAddress("Bcc", adr));
                    }
                }
                mail.Subject = subject;
                var body = new TextPart("html")
                {
                    Text = htmlBody,
                };
                var mult = new Multipart("mixed")
                {
                    body
                };
                mail.Body = mult;
                await SendAsync(mail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        // Control only 3 thread could enter send email function to avoid 'sender thread exceeded' exception 
        private static readonly Semaphore _semaphore = new Semaphore(3, 3);
        private async Task SendAsync(MimeMessage mail)
        {
            try
            {
                _semaphore.WaitOne();
                // DO NOT send email to customer in development and staging environment 
                string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (env.ToLower().Equals("development"))
                {
                    var toBeRemoved = new List<InternetAddress>();
                    foreach (var address in mail.To)
                    {
                        if (!address.ToString().Contains("pacvue.cn")
                            && !address.ToString().Contains("qq.com")
                            && !address.ToString().Contains("pacvue.com"))
                        {
                            toBeRemoved.Add(address);
                        }
                    }
                    toBeRemoved.ForEach(x =>
                    {
                        mail.To.Remove(x);
                    });
                }
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_host, _port, true);
                    await client.AuthenticateAsync(_username, _password);
                    await client.SendAsync(mail);
                }
                _semaphore.Release();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
