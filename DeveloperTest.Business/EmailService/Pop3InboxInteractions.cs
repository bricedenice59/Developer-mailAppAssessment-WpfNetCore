using DeveloperTest.EmailDiscovery.ConnectionService;
using DeveloperTest.Helpers.Models;
using Limilabs.Client.POP3;
using Limilabs.Mail;
using Microsoft.Extensions.Logging;

namespace DeveloperTest.EmailDiscovery.EmailService
{
    internal class Pop3InboxInteractions : IEmailInboxInteractions
    {
        public string ProtocolType => Protocols.POP3.ToString();

        private readonly ILogger _logger;

        public Pop3InboxInteractions(ILogger<Pop3InboxInteractions> logger)
        {
            _logger = logger;
        }

        public async Task<(string, string)> DownloadBodyAsync(string emailUid, IConnection connection)
        {
            ArgumentNullException.ThrowIfNull(connection, nameof(connection));

            string text = null, html = null;
            var pop3Cnx = connection.ConnectionObject as Pop3;
            try
            {
                MailBuilder builder = new MailBuilder();
                var resMessage = await pop3Cnx!.GetMessageByUIDAsync(emailUid);
                IMail email = builder.CreateFromEml(resMessage);

                if (email.Text != null)
                    text = email.Text;
                if (email.Html != null)
                    html = email.Html;
            }
            catch (Exception e)
            {
                _logger.LogError("An error occurred when trying to download email body", e);
            }

            return (html, text);
        }

        public async Task<EmailModel> DownloadHeaderAsync(string emailUid, IConnection connection)
        {
            ArgumentNullException.ThrowIfNull(connection, nameof(connection));

            var pop3Cnx = connection.ConnectionObject as Pop3;
            try
            {
                MailBuilder builder = new MailBuilder();
                var emailHeaderInfo = await pop3Cnx!.GetHeadersByUIDAsync(emailUid);
                IMail email = builder.CreateFromEml(emailHeaderInfo);
                return new EmailModel
                {
                    Uid = emailUid,
                    From = email.From.FirstOrDefault()?.Name,
                    Date = email.Date?.ToString(),
                    Subject = email.Subject
                };
            }
            catch (Exception e)
            {
                _logger.LogError("An error occurred when trying to download email body", e);
            }

            return null;
        }

        public async Task<List<string>> GetAllEmailsUIds(IConnection connection)
        {
            ArgumentNullException.ThrowIfNull(connection, nameof(connection));

            var uids = await (connection.ConnectionObject as Pop3)!.GetAllAsync();
            return uids; 
        }

        public Task<bool> SelectInboxAsync(IConnection cnx)
        {
            return Task.FromResult(true);
        }
    }
}
