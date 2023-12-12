using System.Collections.Concurrent;
using DeveloperTest.EmailDiscovery.ConnectionService;
using DeveloperTest.EmailDiscovery.Factory;
using Microsoft.Extensions.Logging;
using static System.Net.Mime.MediaTypeNames;

namespace DeveloperTest.EmailDiscovery.EmailService;

public class EmailConnectionInteraction : IEmailConnectionInteractions
{
    private readonly ILogger _logger;
    private readonly IConnectionFactory _connectionFactory;
    private readonly IConnectionPoolInstance _connectionPoolInstance;

    public EmailConnectionInteraction(ILogger<EmailConnectionInteraction> logger,
        IConnectionFactory connectionFactory,
        IConnectionPoolInstance connectionPoolInstance)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _connectionPoolInstance = connectionPoolInstance;
    }

    #region Create Connection

    public IConnection CreateConnection()
    {
        return _connectionFactory.Create(_connectionPoolInstance.ConnectionDescriptor.MailProtocol.ToString());
    }

    #endregion

    #region Connect && Authenticate

    public async Task<bool> ConnectAndAuthenticateAsync(IConnection cnx)
    {
        _ = cnx ?? throw new ArgumentNullException();

        try
        {
            await cnx.ConnectAsync(_connectionPoolInstance.ConnectionDescriptor);            
        }
        catch (Limilabs.Client.ServerException serverException)
        {
            _logger.LogError($"Could not connect to host server! connection id {cnx.ConnectionId}", serverException);
            throw;
        }
        try
        {
            await cnx.AuthenticateAsync(_connectionPoolInstance.ConnectionDescriptor);
        }
        catch (Limilabs.Client.ServerException serverException)
        {
            _logger.LogError($"Authentication failed for connection id {cnx.ConnectionId}", serverException);
            throw;
        }
        return true;
    }

    #endregion

    #region Disconnect

    /// <summary>
    /// Disconnect connections
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectAsync(IConnection cnx)
    {
        _ = cnx ?? throw new ArgumentNullException();

        try
        {
            await cnx.DisconnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Could not disconnect connection id {cnx.ConnectionId} from email server!", ex);
        }
    }

    #endregion
}