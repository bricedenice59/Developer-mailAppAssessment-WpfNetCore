namespace DeveloperTest.Helpers.Messages;

public class LoadHtmlMessage
{
    public string Html { get; set; }
    public LoadHtmlMessage(string html)
    {
        Html = html;
    }
}
