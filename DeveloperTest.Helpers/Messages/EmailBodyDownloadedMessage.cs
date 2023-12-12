using DeveloperTest.Helpers.Models;

namespace DeveloperTest.Helpers.Messages;

public class EmailBodyDownloadedMessage
{
    public EmailModel EmailObj { get; set; }
    public bool IsBusy { get; set; }

    public EmailBodyDownloadedMessage(EmailModel emailObj, bool isBusy)
    {
        EmailObj = emailObj;
        IsBusy = isBusy;
    }
}
