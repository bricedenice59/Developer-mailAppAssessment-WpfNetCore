using DeveloperTest.EmailDiscovery.ConnectionService;
using DeveloperTest.EmailDiscovery.EmailService;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeveloperTest.EmailDiscovery.Factory
{
    public static class FactoryExtension
    {
        public static IServiceCollection AddConnectionFactory(this IServiceCollection services)
        {
            return services
                .AddTransient<IConnection, ImapConnection>()
                .AddTransient<IConnection, Pop3Connection>()
                .AddSingleton<Func<IEnumerable<IConnection>>>
                             (x => () => x.GetService<IEnumerable<IConnection>>()!)
                .AddTransient<IEmailInboxInteractions, ImapInboxInteractions>()
                .AddTransient<IEmailInboxInteractions, Pop3InboxInteractions>()
                .AddSingleton<Func<IEnumerable<IEmailInboxInteractions>>>
                             (x => () => x.GetService<IEnumerable<IEmailInboxInteractions>>()!)
                .AddSingleton<IConnectionFactory, ConnectionFactory>()
                .AddSingleton<IEmailInboxFactory, EmailInboxFactory>();
        }
    }
}
