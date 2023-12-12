
using System.Collections.Concurrent;
using DeveloperTest.EmailDiscovery.ConnectionService;
using DeveloperTest.Helpers.Events;
using DeveloperTest.Helpers.Models;

namespace DeveloperTest.EmailDiscovery.EmailService;

public interface IEmailDownloadService
{
    event EventHandler<ScanEmailsStatusChangedEventArgs> ScanEmailsStatusChanged;
    event EventHandler<NewEmailDiscoveredEventArgs> NewEmailDiscovered;
    Task DownloadEmails();
    Task DownloadAndSetBodyAsync(EmailModel emailObj, IConnection connection);
    ConcurrentBag<string> ProcessedBodies { get; }
}
