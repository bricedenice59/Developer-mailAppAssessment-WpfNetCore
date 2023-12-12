using DeveloperTest.EmailDiscovery.ConnectionService;
using DeveloperTest.Helpers.Models;
using Limilabs.Client.IMAP;
using Microsoft.Extensions.Logging;

namespace DeveloperTest.EmailDiscovery.EmailService
{
    public class ImapInboxInteractions : IEmailInboxInteractions
    {
        public string ProtocolType => Protocols.IMAP.ToString();

        private readonly ILogger _logger;

        public ImapInboxInteractions(ILogger<ImapInboxInteractions> logger)
        {
            _logger = logger;
        }

        public async Task<(string, string)> DownloadBodyAsync(string emailUid, IConnection connection)
        {
            ArgumentNullException.ThrowIfNull(connection, nameof(connection));

            // Download only text and html parts
            string text = null, html = null;
            var imapCnx = connection.ConnectionObject as Imap;
            try
            {
                var emailId = long.Parse(emailUid);
                var emailBodyStruct = await imapCnx!.GetBodyStructureByUIDAsync(emailId);

                if (emailBodyStruct.Text != null)
                    text = await imapCnx.GetTextByUIDAsync(emailBodyStruct.Text);
                if (emailBodyStruct.Html != null)
                    html = await imapCnx.GetTextByUIDAsync(emailBodyStruct.Html);
            }
            catch (Exception e)
            {
                _logger.LogError("An error occurred when trying to download email header", e);
            }

            return (html, text);
        }

        public async Task<EmailModel> DownloadHeaderAsync(string emailUid, IConnection connection)
        {
            ArgumentNullException.ThrowIfNull(connection, nameof(connection));

            try
            {
                var emailId = long.Parse(emailUid);
                var emailHeaderInfo = await (connection.ConnectionObject as Imap)!.GetMessageInfoByUIDAsync(emailId);

                return new EmailModel()
                {
                    Uid = emailUid,
                    From = emailHeaderInfo.Envelope.From.FirstOrDefault()?.Address,
                    Date = emailHeaderInfo.Envelope.Date?.ToString(),
                    Subject = emailHeaderInfo.Envelope.Subject
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

            List<string> uids = new();
            var lstUidsLong = await (connection.ConnectionObject as Imap)!.SearchAsync(Flag.All);

            //really don't like this but requesting for emails ids for different protocols returns different object types
            //1.imap returns me a List<long>
            //2.pop3 returns me a List<string>
            lstUidsLong.ForEach(x => uids.Add(x.ToString()));

            _logger.LogInformation($"API returns emails sorted from oldest to newest");
            _logger.LogInformation($"Let's sort them in the opposite way(from newest to oldest) right now, so we don't need to deal with it later in UI");

            //for my testings on
            //1.retrieving gmail emails with imap protocol returns me emails sorted from newest to oldest
            //2.retrieving hotmail emails with imap protocol returns me emails sorted from oldest to newest
            //what is going with this API, that's a bug or the API documentation is wrong

            //reverse randomly ? hihi :)
            uids.Reverse();

            return uids;
        }

        public async Task<bool> SelectInboxAsync(IConnection connection)
        {
            ArgumentNullException.ThrowIfNull(connection, nameof(connection));

            try
            {
                await ((Imap)connection.ConnectionObject).SelectInboxAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Can not select inbox", ex);
            }

            return false;
        }
    }
}
