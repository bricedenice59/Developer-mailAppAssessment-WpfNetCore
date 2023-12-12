using DeveloperTest.ViewModels.Popups;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DeveloperTest.ViewModels;

/// <summary>
/// This class contains static references to all the view models in the
/// application and provides an entry point for the bindings.
/// </summary>
public class ViewModelLocator
{
    /// <summary>
    /// Initializes a new instance of the ViewModelLocator class.
    /// </summary>
    public ViewModelLocator()
    {
  
    }

    public EmailsDataViewModel EmailsDataViewModel => App.ServiceProvider.GetService<EmailsDataViewModel>();
    public EmailsBodyDataViewModel EmailsBodyDataViewModel => App.ServiceProvider.GetService<EmailsBodyDataViewModel>();
    public ServerConnectionPropertiesViewModel ServerConnectionProperties => App.ServiceProvider.GetService<ServerConnectionPropertiesViewModel>();
    public ErrorPopupViewModel ErrorPopup => App.ServiceProvider.GetService<ErrorPopupViewModel>();
    public static void Cleanup()
    {
        // TODO Clear the ViewModels
    }
}