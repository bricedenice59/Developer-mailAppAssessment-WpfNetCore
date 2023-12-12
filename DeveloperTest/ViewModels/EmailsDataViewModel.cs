using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DeveloperTest.EmailDiscovery.ConnectionService;
using DeveloperTest.EmailDiscovery.EmailService;
using DeveloperTest.Helpers.Events;
using DeveloperTest.Helpers.Messages;
using DeveloperTest.Helpers.Models;
using DeveloperTest.Utils.Extensions;
using DeveloperTest.Utils.WPF;
using DeveloperTest.Utils.WPF.Events;
using DeveloperTest.ViewModels.Popups;
using Microsoft.Extensions.Logging;
using MvvmDialogs;


namespace DeveloperTest.ViewModels;

public class EmailsDataViewModel : CommonViewModel
{
    #region Fields

    private readonly IEmailConnectionInteractions _connectionUtils;
    private readonly IEmailInboxInteractions _emailInboxInteractions;
    private readonly IEmailDownloadService _emailDownloadService;
    private readonly IConnectionPoolInstance _connectionPoolInstance;
    private readonly IDialogService _dialogService;
    private EmailModel _selectedItem;
    private string _currentStatus;
    private bool _showAnimationStatus;

    private readonly EventHandler<ScanEmailsStatusChangedEventArgs> _scanEmailsStatusChangedEventHandler;
    private readonly EventHandler<NewEmailDiscoveredEventArgs> _newEmailDiscoveredEventHandler;

    #endregion

    #region Properties

    public string CurrentStatus
    {
        get => _currentStatus;
        set
        {
            SetProperty(ref _currentStatus, value);
        }
    }

    public bool ShowStatusAnimation
    {
        get => _showAnimationStatus;
        set
        {
            SetProperty(ref _showAnimationStatus, value);
        }
    }

    public ObservableCollection<EmailModel> EmailsList { get; }

    public EmailModel SelectedItem
    {
        get => _selectedItem;
        set
        {
            //avoid going further with multiple clicks when the item clicked in datagrid is already selected
            if (value == _selectedItem)
                return;

            SetProperty(ref _selectedItem, value);

            //if body not downloaded yet, then request to download it.
            if (!value.IsBodyDownloaded)
            {
                //notify UI we have a busy download
                Messenger.Send(new EmailBodyDownloadedMessage(value, true));
                Logger.LogInformation($"Email body with id:{value.Uid} has not been downloaded yet");

                //subscribe event to be later notified once the message is downloaded and then notify UI
                value.OnEmailBodyDownloaded += OnEmailBodyDownloaded;

                //let's force downloading it
                Messenger.Send(new RequestDownloadOnDemandMessage(value));
            }
            else
            {
                Messenger.Send(new EmailBodyDownloadedMessage(value, false));
                Logger.LogInformation($"Body already downloaded for id:{value.Uid}");
            }
        }
    }

    private void OnEmailBodyDownloaded(object sender, DownloadBodyFinishedEventArgs e)
    {
        //only update UI if the current selected item is still the same, otherwise ignore
        if (_selectedItem.Uid == e.Email.Uid)
            Messenger.Send(new EmailBodyDownloadedMessage(e.Email, false));
        e.Email.OnEmailBodyDownloaded -= OnEmailBodyDownloaded;
    }

    #endregion

    #region Ctor

    public EmailsDataViewModel(ILogger<EmailsDataViewModel> logger,
        IMessenger messenger,
        IDialogService dialogService,
        IEmailConnectionInteractions connectionUtils,
        IEmailInboxInteractions emailInboxInteractions,
        IEmailDownloadService emailDownloadService,
        IConnectionPoolInstance connectionPoolInstance) : base(logger, messenger)
    {
        _connectionUtils = connectionUtils;
        _emailInboxInteractions = emailInboxInteractions;
        _emailDownloadService = emailDownloadService;
        _connectionPoolInstance = connectionPoolInstance;
        _dialogService = dialogService;

        EmailsList = new ObservableCollection<EmailModel>();
        CurrentStatus = null;
        ShowStatusAnimation = false;

        _newEmailDiscoveredEventHandler = EventHandlerHelper.SafeEventHandler<NewEmailDiscoveredEventArgs>(CallbackOnNewEmailDiscovered);
        _scanEmailsStatusChangedEventHandler = EventHandlerHelper.SafeEventHandler<ScanEmailsStatusChangedEventArgs>(CallbackOnScanEmailsStatusChanged);

        _emailDownloadService.NewEmailDiscovered += _newEmailDiscoveredEventHandler;
        _emailDownloadService.ScanEmailsStatusChanged += _scanEmailsStatusChangedEventHandler;

        messenger.Register<StartScanEmailMessage>(this, async (r, m) =>
        {
            await _emailDownloadService.DownloadEmails();
        });
        messenger.Register<RequestDownloadOnDemandMessage>(this, async (r, m) =>
        {
            await DownloadOnDemand(m.EmailObj);
        });
    }

    #endregion

    #region Events implementation

    private async void CallbackOnScanEmailsStatusChanged(object o, ScanEmailsStatusChangedEventArgs e)
    {
        switch (e.Status)
        {
            case ScanProgress.InProgress:
                ShowStatusAnimation = true;
                CurrentStatus = "Download in progress...";
                return;
            case ScanProgress.Completed:
                CurrentStatus = "Download completed ! :)";
                ShowStatusAnimation = false;
                _emailDownloadService.NewEmailDiscovered -= _newEmailDiscoveredEventHandler;
                _emailDownloadService.ScanEmailsStatusChanged -= _scanEmailsStatusChangedEventHandler;

                if (_emailDownloadService.ProcessedBodies.Count != _emailDownloadService.ProcessedBodies.Distinct().Count())
                {
                    // Duplicates exist and that's an issue as this means some bodies have been downloaded more than once
                    var errorPopupViewModel = new ErrorPopupViewModel(Logger)
                    {
                        Message = $"Some bodies have been downloaded more than once! \r\n Found {_emailDownloadService.ProcessedBodies.Distinct().Count()} distinct bodies downloaded but {_emailDownloadService.ProcessedBodies.Count} processed!!"
                    };

                    await _dialogService.ShowDialogAsync(this, errorPopupViewModel);
                }

                _connectionPoolInstance.Connections.ForEach(async cnx => await _connectionUtils.DisconnectAsync(cnx));

                return;
        }
    }

    private void CallbackOnNewEmailDiscovered(object o, NewEmailDiscoveredEventArgs e)
    {
        if (e?.Email != null)
            EmailsList.Add(e.Email);
    }

    #endregion

    #region Methods

    private Task DownloadOnDemand(EmailModel emailObj)
    {
        return Task.Run(async () =>
        {
            //allocate new connection
            var newConnection = _connectionUtils.CreateConnection();

            //then connect and authenticate
            bool connectAndAuthenticateSuccess = true;
            try
            {
                await _connectionUtils.ConnectAndAuthenticateAsync(newConnection);
                await _emailInboxInteractions.SelectInboxAsync(newConnection);
            }
            catch (Exception e)
            {
                connectAndAuthenticateSuccess = false;
                Logger.LogError("Something went wrong when trying to create a new connection for downloading an item on demand.", e);
            }

            //download body
            if (connectAndAuthenticateSuccess)
                await _emailDownloadService.DownloadAndSetBodyAsync(emailObj, newConnection);

            //close and dispose connection
            await _connectionUtils.DisconnectAsync(newConnection);
        });
    }

    #endregion
}