using DeveloperTest.Helpers.Models;

namespace DeveloperTest.EmailDiscovery.ConnectionService;

public interface IConnectionDescriptor
{
    string Server { get; set; }
    int Port { get; set; }
    string Username { get; set; }
    string Password { get; set; }

    EncryptionTypes EncryptionType { get; set; }

    Protocols MailProtocol { get; set; }

    int MaxActiveConcurrentConnections { get; }
}

public class ConnectionDescriptor : IConnectionDescriptor
{

    #region Properties

    public string Server { get; set; }
    public int Port { get; set; }
    public EncryptionTypes EncryptionType { get; set; }
    public Protocols MailProtocol { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }

    public int MaxActiveConcurrentConnections
    {
        get
        {
            switch (MailProtocol)
            {
                case Protocols.IMAP:
                    return 5;
                case Protocols.POP3:
                    return 3;
                default: return 1;
            }
        }
    }

    #endregion
}