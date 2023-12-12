using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DeveloperTest.Helpers.Messages;
using DeveloperTest.Utils.WPF;
using Microsoft.Extensions.Logging;

namespace DeveloperTest.ViewModels;

public partial class EmailsBodyDataViewModel : CommonViewModel
{
    #region Fields

    [ObservableProperty]
    private bool _isBodyAvailable;

    [ObservableProperty]
    private bool _showThisView;

    private bool _hasWebviewRenderingError;
    #endregion

    #region Properties


    public bool HasWebviewRenderingError
    {
        get => _hasWebviewRenderingError;
        set
        {
            SetProperty(ref _hasWebviewRenderingError, value);
            if (value)
                ShowThisView = false;
        }
    }

    #endregion

    #region Ctor

    public EmailsBodyDataViewModel(ILogger<EmailsBodyDataViewModel> logger, IMessenger messenger) 
        : base(logger,messenger)
    {
        IsBodyAvailable = false;
        ShowThisView = false;
        HasWebviewRenderingError = false;

        messenger.Register<EmailBodyDownloadedMessage>(this, (r, m) =>
        {
            HasWebviewRenderingError = false;
            IsBodyAvailable = !m.IsBusy;
            if (!_showThisView)
                ShowThisView = true;

            if (m.EmailObj.IsBodyDownloaded)
            {
                messenger.Send(new LoadHtmlMessage(m.EmailObj.Body));
            }
        });
    }

    #endregion
}