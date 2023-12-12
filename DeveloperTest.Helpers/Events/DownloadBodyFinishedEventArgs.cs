using DeveloperTest.Helpers.Models;

namespace DeveloperTest.Helpers.Events;

public class DownloadBodyFinishedEventArgs : EventArgs
{
    public EmailModel Email { get; }

    public DownloadBodyFinishedEventArgs(EmailModel emailObj)
    {
        Email = emailObj;
    }
}
