using System.Threading;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DeveloperTest.Helpers.Logging;
using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Serilog;
using DeveloperTest.Utils.WPF;

namespace DeveloperTest;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider;
    private const string AppGuid = "96f075aa-dead-4878-a73f-46ece7b10f06";
    private IHost _host;
    static App()
    {
        DispatcherHelper.Initialize();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration((hostBuilderContext, configurationBuilder)
            => configurationBuilder.AddUserSecrets(typeof(App).Assembly))
        .ConfigureServices((hostContext, services)
            => services
                .AddApplicationLogging(hostContext)
                .AddApplicationDialogService()
                .AddApplicationViews()
                .AddBusinessService()
                .AddMessenger<WeakReferenceMessenger>());

    protected override async void OnStartup(StartupEventArgs e)
    {
        Log.Logger = ServiceCollectionExtensions.StartupLogger();

        if (CheckForRunningApp())
        {
            Log.Information("An instance of this program is already running...");
            Current.Shutdown();
            return;
        }

        try
        {
            _host = CreateHostBuilder(null).Build();
            await _host.StartAsync(CancellationToken.None).ConfigureAwait(true);

            ServiceProvider = _host.Services;

            var logger = _host.Services.GetRequiredService<ILogger<App>>();
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                logger.UnhandledException(args.ExceptionObject as Exception);
                Dispose();
            };

            this.DispatcherUnhandledException += (sender, args) =>
            {
                logger.UnhandledException(args.Exception);
                Dispose();
            };

            this.MainWindow = _host.Services.GetRequiredService<MainWindow>();
            this.MainWindow.Visibility = Visibility.Visible;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly.");
        }
    }

    private void Dispose()
    {
        Log.CloseAndFlush();
        _host?.Dispose();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Dispose();

        base.OnExit(e);
    }


    protected static bool CheckForRunningApp()
    {
        // Allow only one instance of the software to be run.
        var mutex = new Mutex(false, @"Global\" + AppGuid);
        return !mutex.WaitOne(0, false);
    }
}
