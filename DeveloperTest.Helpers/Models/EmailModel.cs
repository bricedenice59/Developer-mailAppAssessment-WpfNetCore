using System;
using System.Threading;
using DeveloperTest.Helpers.Events;

namespace DeveloperTest.Helpers.Models;

public class EmailModel
{
    #region Properties

    public string Uid { get; set; }
    public string From { get; set; }
    public string Subject { get; set; }
    public string Date { get; set; }

    public string Body { get; set; }

    public bool IsBodyDownloaded { get; set; }

    #endregion

    #region Events
    public event EventHandler<DownloadBodyFinishedEventArgs> OnEmailBodyDownloaded;
    #endregion

    public void SetBodyIsNowDownloaded()
    {
        IsBodyDownloaded = true;

        OnEmailBodyDownloaded?.Invoke(this, new DownloadBodyFinishedEventArgs(this));
    }
}