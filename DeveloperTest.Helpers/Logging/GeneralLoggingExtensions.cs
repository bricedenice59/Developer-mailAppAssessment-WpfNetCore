using Microsoft.Extensions.Logging;

namespace DeveloperTest.Helpers.Logging;

public static partial class GeneralLoggingExtensions
{
    [LoggerMessage(850, LogLevel.Error, "An unhandled exception occured.")]
    public static partial void UnhandledException(this ILogger logger, Exception? ex = null);
}
