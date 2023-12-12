using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DeveloperTest.Utils.WPF;
using Microsoft.Extensions.Logging;
using MvvmDialogs;


namespace DeveloperTest.ViewModels.Popups;

public class ErrorPopupViewModel : CommonDialogViewModel, IModalDialogViewModel
{
    private ICommand _okCommand;

    public string Message
    {
        get; set;
    }

    public ICommand OkCommand
    {
        get
        {
            return _okCommand ?? (_okCommand = new RelayCommand(() =>
            {
                DialogResult = false;
            }));
        }
    }

    public ErrorPopupViewModel(ILogger logger) : base(logger)
    {
    }
}
