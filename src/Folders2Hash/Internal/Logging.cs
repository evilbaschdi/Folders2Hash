using System.Collections.Concurrent;
using System.Text;
using EvilBaschdi.Core.Logging;
using Folders2Hash.Models;
using Microsoft.Extensions.Logging;

namespace Folders2Hash.Internal;

/// <inheritdoc />
public class Logging : ILogging
{
    private readonly ILogFilePathWithInterval _logFilePathWithInterval;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="logFilePathWithInterval"></param>
    public Logging(ILogFilePathWithInterval logFilePathWithInterval)
    {
        _logFilePathWithInterval = logFilePathWithInterval ?? throw new ArgumentNullException(nameof(logFilePathWithInterval));
    }

    /// <inheritdoc />
    public void RunFor(ConcurrentBag<LogEntry> logEntries, Configuration configuration)
    {
        ArgumentNullException.ThrowIfNull(logEntries);
        ArgumentNullException.ThrowIfNull(configuration);

        var stringBuilder = new StringBuilder();

        foreach (var logEntry in logEntries.OrderBy(x => x.FileName).ThenBy(x => x.Type))
        {
            stringBuilder.Append($"{logEntry.FileName};{logEntry.Type};{logEntry.HashSum};{logEntry.AlreadyExisting};{Environment.NewLine}");
        }

        var path = _logFilePathWithInterval.ValueFor(configuration.LoggingPath, "Folders2Hash_Log", LoggingFileInterval.None, DateTime.Now);
        var logger = new SimpleFileLogger(path, LogLevel.Information, new object(), "FileName;Type;HashSum;AlreadyExisting;");
        logger.LogInformation("{LogData}", stringBuilder.ToString().TrimEnd());
    }

    /// <inheritdoc />
    public void RunFor(string file, string message, Configuration configuration)
    {
        var path = _logFilePathWithInterval.ValueFor(configuration.LoggingPath, "Folders2Hash_ErrorLog", LoggingFileInterval.None, DateTime.Now);
        var logger = new SimpleFileLogger(path, LogLevel.Error, new object(), "FileName;ErrorMessage;");
        logger.LogError("{File};{Message}", file, message);
    }
}