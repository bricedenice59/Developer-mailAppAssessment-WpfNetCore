using Limilabs.Client.POP3;
using Microsoft.Extensions.Logging;
using DeveloperTest.Helpers.Models;
using Limilabs.Client;

namespace DeveloperTest.EmailDiscovery.ConnectionService;

public class Pop3Connection : IConnection
{
    private Pop3 _pop3ConnectionObj = new();

    private long _isBusyValue = 0;

    public Guid ConnectionId { get; }

    public bool IsBusy
    {
        get => Interlocked.Read(ref _isBusyValue) == 1;
        set => Interlocked.Exchange(ref _isBusyValue, Convert.ToInt64(value));
    }

    protected ILogger Logger { get; }

    public string ProtocolType => Protocols.POP3.ToString();

    public ClientBase ConnectionObject => _pop3ConnectionObj;

    public Pop3Connection(ILogger<Pop3Connection> logger)
    {
        Logger = logger;
        IsBusy = false;
        ConnectionId = Guid.NewGuid();
    }

    public async Task ConnectAsync(IConnectionDescriptor cd)
    {
        Logger.LogInformation($"Connection #{ConnectionId} Try connecting to Pop3 mail server {cd.Server}:{cd.Port} :");
        switch (cd.EncryptionType)
        {
            case EncryptionTypes.SSLTLS:
                await _pop3ConnectionObj.ConnectSSLAsync(cd.Server, cd.Port);
                break;
            case EncryptionTypes.STARTTLS:
                await _pop3ConnectionObj.ConnectAsync(cd.Server, cd.Port);
                if (_pop3ConnectionObj.SupportedExtensions().Contains(Pop3Extension.STLS))
                    await _pop3ConnectionObj.StartTLSAsync();
                break;
            case EncryptionTypes.Unencrypted:
                await _pop3ConnectionObj.ConnectAsync(cd.Server, cd.Port);
                break;
        }

        if (_pop3ConnectionObj.Connected)
            Logger.LogInformation(_pop3ConnectionObj.ServerGreeting.Message);
    }

    public async Task AuthenticateAsync(IConnectionDescriptor cd)
    {
        if (!_pop3ConnectionObj.Connected)
        {
            Logger.LogInformation("Cannot authenticate as connection with Pop3; connection to server is down");
            return;
        }
        Logger.LogInformation($"Connection #{ConnectionId} Try authenticating with username and password for Pop3 mail server");

        await _pop3ConnectionObj.LoginAsync(cd.Username, cd.Password);
        Logger.LogInformation($"Connection #{ConnectionId} Authentication OK");
    }

    public async Task DisconnectAsync()
    {
        if (_pop3ConnectionObj == null)
            return;

        Logger.LogInformation($"Connection #{ConnectionId} Try disconnecting from Pop3 mail server");
        await _pop3ConnectionObj.CloseAsync(false);
    }
}