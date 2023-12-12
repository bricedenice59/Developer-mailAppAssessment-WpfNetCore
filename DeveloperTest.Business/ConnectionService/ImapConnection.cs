using Limilabs.Client.IMAP;
using Microsoft.Extensions.Logging;
using DeveloperTest.Helpers.Models;
using Limilabs.Client;

namespace DeveloperTest.EmailDiscovery.ConnectionService;

public class ImapConnection : IConnection
{
    private Imap _imapConnectionObj = new();

    private long _isBusyValue = 0;

    public Guid ConnectionId { get; }

    public bool IsBusy
    {
        get => Interlocked.Read(ref _isBusyValue) == 1;
        set => Interlocked.Exchange(ref _isBusyValue, Convert.ToInt64(value));
    }

    protected ILogger Logger { get; }

    public string ProtocolType => Protocols.IMAP.ToString();

    public ClientBase ConnectionObject => _imapConnectionObj;

    public ImapConnection(ILogger<ImapConnection> logger)
    {
        Logger = logger;
        IsBusy = false;
        ConnectionId = Guid.NewGuid();
    }

    /// <summary>
    /// Initiate connection to Email server 
    /// </summary>
    /// <returns></returns>
    public async Task ConnectAsync(IConnectionDescriptor cd)
    {
        Logger.LogInformation($"Connection #{ConnectionId} Try connecting to Imap mail server {cd.Server}:{cd.Port} :");

        switch (cd.EncryptionType)
        {
            case EncryptionTypes.SSLTLS:
                await _imapConnectionObj.ConnectSSLAsync(cd.Server, cd.Port);
                break;
            case EncryptionTypes.STARTTLS:
                await _imapConnectionObj.ConnectAsync(cd.Server, cd.Port);
                await _imapConnectionObj.StartTLSAsync();
                break;
            case EncryptionTypes.Unencrypted:
                await _imapConnectionObj.ConnectAsync(cd.Server, cd.Port);
                break;
        }

        if (_imapConnectionObj.Connected)
            Logger.LogInformation(_imapConnectionObj.ServerGreeting.Message);
    }

    /// <summary>
    /// Authenticate with email server with provided credentials  
    /// </summary>
    /// <returns></returns>
    public async Task AuthenticateAsync(IConnectionDescriptor cd)
    {
        if (!_imapConnectionObj.Connected)
        {
            Logger.LogInformation("Cannot authenticate as connection with Imap; connection to server is down");
            return;
        }
        Logger.LogInformation($"Connection #{ConnectionId} Try authenticating with username and password for Imap mail server");

        await _imapConnectionObj.UseBestLoginAsync(cd.Username, cd.Password);
        Logger.LogInformation($"Connection #{ConnectionId} Authentication OK");
    }

    public async Task DisconnectAsync()
    {
        if (_imapConnectionObj == null)
            return;

        Logger.LogInformation($"Connection #{ConnectionId} Try disconnecting from Imap mail server");
        await _imapConnectionObj.CloseAsync(false);
    }
}