using DeveloperTest.EmailDiscovery.ConnectionService;
using DeveloperTest.Helpers.Models;


namespace DeveloperTest.EmailDiscovery.EmailService;

public interface IEmailInboxInteractions
{
    public string ProtocolType { get; }

    Task<bool> SelectInboxAsync(IConnection cnx);

    Task<List<string>> GetAllEmailsUIds(IConnection cnx);

    Task<EmailModel> DownloadHeaderAsync(string emailUid, IConnection connection);

    Task<(string,string)> DownloadBodyAsync(string emailUid, IConnection connection);
}
