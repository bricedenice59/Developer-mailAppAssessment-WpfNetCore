using DeveloperTest.Helpers.Models;

namespace DeveloperTest.Helpers.Messages;

public class RequestDownloadOnDemandMessage : EventArgs
{
    public EmailModel EmailObj { get; set; }

    public RequestDownloadOnDemandMessage(EmailModel emailObj)
    {
        EmailObj = emailObj;
    }
}
