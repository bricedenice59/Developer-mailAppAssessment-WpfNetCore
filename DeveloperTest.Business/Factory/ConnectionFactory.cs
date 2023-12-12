using DeveloperTest.EmailDiscovery.ConnectionService;
using DeveloperTest.Helpers.Models;

namespace DeveloperTest.EmailDiscovery.Factory;


public interface IConnectionFactory
{
    IConnection Create(string protocolType);
}

public class ConnectionFactory : IConnectionFactory
{
    private readonly Func<IEnumerable<IConnection>> _factory;

    public ConnectionFactory(Func<IEnumerable<IConnection>> factory)
    {
        _factory = factory;
    }

    public IConnection Create(string protocolType)
    {
        ArgumentException.ThrowIfNullOrEmpty(protocolType);

        if (Enum.TryParse(protocolType, out Protocols result))
        {
            var output = _factory();
            return output.First(x => x.ProtocolType.Equals(result.ToString()));
        }
        throw new NotImplementedException($"Factory: Cannot create IConnection instance for protocol {protocolType}");
    }
}
