using Limilabs.Client;

namespace DeveloperTest.EmailDiscovery.ConnectionService;

public interface IConnection : IConnectionActions
{
    Guid ConnectionId { get; }

    bool IsBusy { get; set; }

    string ProtocolType { get; }

    ClientBase ConnectionObject { get; }
}
