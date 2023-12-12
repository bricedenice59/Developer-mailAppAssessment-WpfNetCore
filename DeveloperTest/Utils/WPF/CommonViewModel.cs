using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;


namespace DeveloperTest.Utils.WPF;

public abstract class CommonViewModel : ObservableRecipient
{
    protected readonly ILogger Logger;

    private ICommand _onLoadCommand;

    public ICommand OnLoadCommand { get { return _onLoadCommand ?? (_onLoadCommand = new RelayCommand(async () => await ExecuteOnLoad())); } }

    protected new readonly IMessenger Messenger;

    /// <summary>
    /// Method executed on window load event.
    /// </summary>
    protected virtual async Task ExecuteOnLoad()
    {
    }

    public CommonViewModel(ILogger logger)
    {
        Logger = logger;
    }

    public CommonViewModel(ILogger logger, IMessenger messenger)
    {
        Messenger = messenger;
        Logger = logger;
    }
}
