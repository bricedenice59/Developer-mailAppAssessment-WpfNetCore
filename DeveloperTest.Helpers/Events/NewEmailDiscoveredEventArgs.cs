using DeveloperTest.Helpers.Models;

namespace DeveloperTest.Helpers.Events;

public class NewEmailDiscoveredEventArgs : EventArgs
{
    public EmailModel Email { get; }


    public NewEmailDiscoveredEventArgs(EmailModel emailObj)
    {
        Email = emailObj;
    }
}
