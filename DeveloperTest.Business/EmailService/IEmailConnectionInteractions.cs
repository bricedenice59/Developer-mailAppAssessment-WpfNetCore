using System.Collections.Generic;
using System.Threading.Tasks;
using DeveloperTest.EmailDiscovery.ConnectionService;

namespace DeveloperTest.EmailDiscovery.EmailService;

public interface IEmailConnectionInteractions
{
    IConnection CreateConnection();
    Task<bool> ConnectAndAuthenticateAsync(IConnection cnx);
    Task DisconnectAsync(IConnection cnx);
}
