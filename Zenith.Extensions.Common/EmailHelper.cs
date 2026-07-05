using System.Net;
using System.Net.Mail;
namespace Zenith.Extensions.Common

{
    public class EmailHelper
    {
        private readonly SmtpClient _client;
        private readonly string _username;

        public EmailHelper() : this(false)
        {

        }

        public EmailHelper(bool isCN)
        {
          
            _username = ConfigurationHelper.GetValue("Email:Username");
            string password = ConfigurationHelper.GetValue("Email:Password");
            string host = ConfigurationHelper.GetValue("Email:Host");
            int port = Convert.ToInt32(ConfigurationHelper.GetValue("Email:Port"));
            _client = new SmtpClient
            {
                Host = host,
                Port = port,
                Credentials = new NetworkCredential(_username, password),
                EnableSsl = true
            };
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
                using (MailMessage mail = new MailMessage
                {
                    From = new MailAddress(_username, "Pacvue"),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                })
                {
                    if (attachmentsFilePath != null)
                    {
                        foreach (string filePath in attachmentsFilePath)
                        {
                            mail.Attachments.Add(new Attachment(filePath));
                            //using (var stream = System.IO.File.OpenRead(filePath))
                            //{
                            //    string fileName = System.IO.Path.GetFileName(filePath);
                            //    mail.Attachments.Add(new Attachment(stream, fileName));
                            //}
                        }
                    }
                    foreach (string adr in receiveAdrs)
                    {
                        mail.To.Add(new MailAddress(adr));
                    }
                    if (bccs != null)
                    {
                        foreach (string adr in bccs)
                        {
                            mail.Bcc.Add(new MailAddress(adr));
                        }
                    }
                    await SendAsync(mail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        // Control only 3 thread could enter send email function to avoid 'sender thread exceeded' exception 
        private static readonly Semaphore _semaphore = new Semaphore(3, 3);
        private async Task SendAsync(MailMessage mail)
        {
            try
            {
                _semaphore.WaitOne();
                // DO NOT send email to customer in development and staging environment 
                string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                if (env.ToLower().Equals("development"))
                {
                    var toBeRemoved = new List<MailAddress>();
                    foreach (var address in mail.To)
                    {
                        string host = address.Address.Split('@')[1];
                        if (host != "pacvue.cn" && host != "qq.com" && host != "pacvue.com")
                        {
                            toBeRemoved.Add(address);
                        }
                    }
                    toBeRemoved.ForEach(x =>
                    {
                        mail.To.Remove(x);
                    });
                }
                await _client.SendMailAsync(mail);
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
