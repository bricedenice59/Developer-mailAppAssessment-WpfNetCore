using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


namespace DeveloperTest.Utils.WPF;

public abstract class CommonDialogViewModel : CommonViewModel
{
    private bool? _dialogResult;

    public bool? DialogResult
    {
        get { return _dialogResult; }
        set { SetProperty(ref _dialogResult, value); }
    }

    protected CommonDialogViewModel(ILogger logger) : base(logger)
    {
    }

    protected override Task ExecuteOnLoad()
    {
        DialogResult = null;
        return base.ExecuteOnLoad();
    }
}
