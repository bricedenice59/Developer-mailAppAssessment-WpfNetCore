using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.EmailDiscovery.ConnectionService
{
    public interface IConnectionActions
    {
        Task AuthenticateAsync(IConnectionDescriptor cd);

        Task ConnectAsync(IConnectionDescriptor cd);

        Task DisconnectAsync();
    }
}
