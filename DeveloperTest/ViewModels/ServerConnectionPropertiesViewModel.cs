using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using MvvmDialogs;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using System.Linq;
using DeveloperTest.Utils.Extensions;
using DeveloperTest.Utils.WPF;
using DeveloperTest.Helpers.Models;
using DeveloperTest.Helpers.Messages;
using DeveloperTest.ViewModels.Popups;
using DeveloperTest.EmailDiscovery.EmailService;
using DeveloperTest.EmailDiscovery.ConnectionService;
using CommunityToolkit.Mvvm.ComponentModel;
using DeveloperTest.Helpers.Utils;

namespace DeveloperTest.ViewModels;

public partial class ServerConnectionPropertiesViewModel : CommonViewModel, IDataErrorInfo
{
    #region Fields

    private string _selectedProtocol;
    private string _selectedEncryptionType;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectServerAndDownloadEmailsCommand))]
    private string _serverName;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectServerAndDownloadEmailsCommand))]
    private string _port;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectServerAndDownloadEmailsCommand))]
    private string _username;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectServerAndDownloadEmailsCommand))]
    private string _password;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectServerAndDownloadEmailsCommand))]
    private bool _btnStartHasBeenClicked;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectServerAndDownloadEmailsCommand))]
    private bool _isProcessing;

    [ObservableProperty]
    private List<string> _protocols;

    [ObservableProperty]
    private List<string> _encryptionTypes;

    [ObservableProperty]
    private string _messageCurrentOperation;


    private readonly IEmailConnectionInteractions _emailConnectionActions;
    private readonly IConnectionPoolInstance _connectionPoolInstance;
    private readonly IDialogService _dialogService;

    #endregion

    #region Properties

    public string SelectedProtocol
    {
        get => _selectedProtocol;
        set
        {
            SetProperty(ref _selectedProtocol, value);
            if (_selectedEncryptionType != null)
                Port = ConnectionPortUtils.GetDefaultPortForProtocol((Protocols)Enum.Parse(typeof(Protocols), value), (EncryptionTypes)Enum.Parse(typeof(EncryptionTypes), _selectedEncryptionType));
        }
    }

    public string SelectedEncryptionType
    {
        get => _selectedEncryptionType;
        set
        {
            SetProperty(ref _selectedEncryptionType, value);
            if (_selectedProtocol != null)
                Port = ConnectionPortUtils.GetDefaultPortForProtocol((Protocols)Enum.Parse(typeof(Protocols), _selectedProtocol), (EncryptionTypes)Enum.Parse(typeof(EncryptionTypes), value));
        }
    }



    #endregion

    #region Ctor

    public ServerConnectionPropertiesViewModel(ILogger<ServerConnectionPropertiesViewModel> logger,
        IMessenger messenger,
        IEmailConnectionInteractions emailConnectionActions,
        IConnectionPoolInstance connectionPoolInstance,
        IDialogService dialogService) : base(logger, messenger)
    {
        _emailConnectionActions = emailConnectionActions;
        _dialogService = dialogService;
        _connectionPoolInstance = connectionPoolInstance;
    }

    #endregion

    #region Methods

    private bool CanStartRetrieveEmails()
    {
        return !string.IsNullOrEmpty(Port) && 
                !string.IsNullOrEmpty(ServerName) &&
               !string.IsNullOrEmpty(Username) &&
               !string.IsNullOrEmpty(Password) &&
               string.IsNullOrEmpty(Error) &&
               !IsProcessing && !BtnStartHasBeenClicked;
    }

    [RelayCommand(CanExecute = nameof(CanStartRetrieveEmails))]
    public async Task OnConnectServerAndDownloadEmails()
    {
        IsProcessing = true;

        var cd = new ConnectionDescriptor
        {
            EncryptionType = (EncryptionTypes)Enum.Parse(typeof(EncryptionTypes), _selectedEncryptionType),
            MailProtocol = (Protocols)Enum.Parse(typeof(Protocols), _selectedProtocol),
            Port = Convert.ToInt32(Port),
            Server = ServerName,
            Username = Username,
            Password = Password
        };
        //save these data for the running program instance as we will need these for later
        _connectionPoolInstance.ConnectionDescriptor = cd;

        #region Connection stage && Authentication stage

        string errorConnectionMessage = null;

        int nbConcurrentConnections = _connectionPoolInstance.ConnectionDescriptor.MaxActiveConcurrentConnections;

        _connectionPoolInstance.Connections.Clear();

        for (int i = 1; i <= nbConcurrentConnections; i++)
        {
            MessageCurrentOperation = $"Connecting... connection #{i}";
            var cnx = _emailConnectionActions.CreateConnection();
            try
            {
                await Retry.DoAsync(async() => await _emailConnectionActions.ConnectAndAuthenticateAsync(cnx), TimeSpan.FromSeconds(1));
                _connectionPoolInstance.Connections.Add(cnx);
            }
            catch (Exception e)
            {
                errorConnectionMessage = e.Message;
            }
        }

        if (_connectionPoolInstance.Connections.Count == 0)
        {
            Logger.LogError("No available connection to work with");
            var errorPopupViewModelConn = new ErrorPopupViewModel(Logger)
            {
                Message = $"Could not connect/authenticate,  host {cd.Server}:{cd.Port}" + "\r\n" + errorConnectionMessage
            };

            await _dialogService.ShowDialogAsync(this, errorPopupViewModelConn);

            IsProcessing = false;
            return;
        }

        if (_connectionPoolInstance.Connections.Count < nbConcurrentConnections)
        {
            Logger.LogWarning("Working with " + _connectionPoolInstance.Connections.Count + "available connections");
        }

        #endregion

        #region Download process

        MessageCurrentOperation = "Downloading emails...";

        Messenger.Send(new StartScanEmailMessage());

        #endregion

        //make the start button not click-able anymore for this test
        BtnStartHasBeenClicked = true;

        IsProcessing = false;
    }

    #endregion

    #region overrides

    protected override Task ExecuteOnLoad()
    {
        Protocols = Enum.GetNames(typeof(Protocols)).ToList();
        EncryptionTypes = Enum.GetNames(typeof(EncryptionTypes)).ToList();

        return base.ExecuteOnLoad();
    }

    #endregion

    #region IDataErrorInfo
    public string this[string columnName]
    {
        get
        {
            Error = string.Empty;
            switch (columnName)
            {
                case nameof(Port):
                    if (Port == null) break;
                    if (!int.TryParse(Port, out int port))
                        Error = "Port is not a valid number!";
                    break;
            }

            return Error;
        }
    }

    public string Error { get; set; }

    #endregion
}
