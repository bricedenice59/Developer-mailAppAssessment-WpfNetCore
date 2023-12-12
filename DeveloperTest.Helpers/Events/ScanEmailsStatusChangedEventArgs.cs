using DeveloperTest.Helpers.Models;

namespace DeveloperTest.Helpers.Events;

public class ScanEmailsStatusChangedEventArgs : EventArgs
{
    public ScanProgress Status { get; }


    public ScanEmailsStatusChangedEventArgs(ScanProgress status)
    {
        Status = status;
    }
}
