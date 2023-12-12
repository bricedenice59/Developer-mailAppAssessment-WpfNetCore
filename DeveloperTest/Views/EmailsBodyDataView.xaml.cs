using System;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using CommunityToolkit.Mvvm.Messaging;
using DeveloperTest.Helpers.Messages;
using DeveloperTest.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DeveloperTest.Views;

/// <summary>
/// Interaction logic for EmailsBodyDataView.xaml
/// </summary>
public partial class EmailsBodyDataView : UserControl
{
    private readonly ILogger _logger;
    private readonly IMessenger _messenger;
    public EmailsBodyDataView()
    {
        InitializeComponent();
        Loaded += EmailsBodyDataView_Loaded;
        _logger = App.ServiceProvider.GetRequiredService<ILogger<EmailsBodyDataView>>();
        _messenger = App.ServiceProvider.GetRequiredService<IMessenger>();
    }

    private void EmailsBodyDataView_Loaded(object sender, RoutedEventArgs e)
    {
        _messenger.Register<LoadHtmlMessage>(this, (r,m) =>
        {
            try
            {
                _webBrowser.LoadHtml(m.Html);
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(
                    () => ((EmailsBodyDataViewModel) this.DataContext).HasWebviewRenderingError = true);

                _logger?.LogError("Exception caught in webbrowser", ex);
            }
        });
    }
}
