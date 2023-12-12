using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Hosting;
using Serilog.Core;
using DeveloperTest.Views.Popups;
using DeveloperTest.Utils.WPF.Components.Popups;
using MvvmDialogs;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection.Extensions;
using DeveloperTest.ViewModels;
using DeveloperTest.EmailDiscovery.Factory;
using DeveloperTest.EmailDiscovery.EmailService;
using DeveloperTest.EmailDiscovery.ConnectionService;
namespace DeveloperTest;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationLogging(this IServiceCollection services, HostBuilderContext hostContext)
    {
        var log = DefaultLoggerConfiguration()
            .ReadFrom.Configuration(hostContext.Configuration)
            .CreateLogger();

        return services.AddSerilog(log);
    }

    public static IServiceCollection AddApplicationViews(this IServiceCollection services)
    {
        return services
            .AddSingleton<MainWindow>()
            .AddTransient<ErrorPopupView>()
            .AddSingleton<ViewModelLocator>()
            .AddSingleton<ServerConnectionPropertiesViewModel>()
            .AddSingleton<EmailsDataViewModel>()
            .AddSingleton<EmailsBodyDataViewModel>()
            ;
    }

    public static IServiceCollection AddMessenger<TMessenger>(this IServiceCollection services)
           where TMessenger : class, IMessenger
    {
        services.TryAddScoped<TMessenger>();
        services.TryAddScoped<IMessenger>(provider => provider.GetRequiredService<TMessenger>());
        return services;
    }

    public static IServiceCollection AddApplicationDialogService(this IServiceCollection services)
    {
        return services.AddSingleton<IDialogService>
            (context => new DialogService(null, new DialogTypeLocator()));
    }

    public static IServiceCollection AddBusinessService(this IServiceCollection services)
    {
        return services
            .AddConnectionFactory()
            .AddSingleton<IConnectionPoolInstance, ConnectionPoolInstance>()
            .AddSingleton<IEmailConnectionInteractions, EmailConnectionInteraction>()
            .AddSingleton<IEmailDownloadService, EmailDownloadService>();
    }


    public static Logger StartupLogger()
        => DefaultLoggerConfiguration().CreateLogger();

    private static LoggerConfiguration DefaultLoggerConfiguration()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/MailApp-Brice-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 15,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}|{Level:u3}|{SourceContext}|{Message:lj}{NewLine}{Exception}")
            ;
    }
}
