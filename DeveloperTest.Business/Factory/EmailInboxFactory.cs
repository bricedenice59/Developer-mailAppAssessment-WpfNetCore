using DeveloperTest.EmailDiscovery.ConnectionService;
using DeveloperTest.EmailDiscovery.EmailService;
using DeveloperTest.Helpers.Models;


namespace DeveloperTest.EmailDiscovery.Factory;

public interface IEmailInboxFactory
{
    IEmailInboxInteractions GetInstance(string protocolType);
}

public class EmailInboxFactory : IEmailInboxFactory
{
    private readonly Func<IEnumerable<IEmailInboxInteractions>> _factory;

    public EmailInboxFactory(Func<IEnumerable<IEmailInboxInteractions>> factory)
    {
        _factory = factory;
    }

    public IEmailInboxInteractions GetInstance(string protocolType)
    {
        ArgumentException.ThrowIfNullOrEmpty(protocolType);

        if (Enum.TryParse(protocolType, out Protocols result))
        {
            var output = _factory();
            return output.First(x => x.ProtocolType.Equals(result.ToString()));
        }
        throw new NotImplementedException($"Factory: Cannot create IEmailInboxInteractions instance for protocol {protocolType}");
    }
}
