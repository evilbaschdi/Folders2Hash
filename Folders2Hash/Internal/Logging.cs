using System.Collections.Concurrent;
using System.Text;
using EvilBaschdi.Core.Logging;
using Folders2Hash.Models;

namespace Folders2Hash.Internal;

/// <inheritdoc />
public class Logging : ILogging
{
    private readonly IAppendAllTextWithHeadline _appendAllTextWithHeadline;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="appendAllTextWithHeadline"></param>
    public Logging(IAppendAllTextWithHeadline appendAllTextWithHeadline)
    {
        _appendAllTextWithHeadline = appendAllTextWithHeadline ?? throw new ArgumentNullException(nameof(appendAllTextWithHeadline));
    }

    /// <inheritdoc />
    public void RunFor(ConcurrentBag<LogEntry> logEntries, Configuration configuration)
    {
        if (logEntries == null)
        {
            throw new ArgumentNullException(nameof(logEntries));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        var stringBuilder = new StringBuilder();

        foreach (var logEntry in logEntries.OrderBy(x => x.FileName).ThenBy(x => x.Type))
        {
            stringBuilder.Append($"{logEntry.FileName};{logEntry.Type};{logEntry.HashSum};{logEntry.AlreadyExisting};{Environment.NewLine}");
        }

        _appendAllTextWithHeadline.RunFor($@"{configuration.LoggingPath}\Folders2Hash_Log_{DateTime.Now:yyyy-MM-dd_HHmm}.csv", stringBuilder,
            "FileName;Type;HashSum;AlreadyExisting;");
    }

    /// <inheritdoc />
    public void RunFor(string file, string message, Configuration configuration)
    {
        _appendAllTextWithHeadline.RunFor($@"{configuration.LoggingPath}\Folders2Hash_ErrorLog_{DateTime.Now:yyyy-MM-dd_HHmm}.csv", $"{file};{message}",
            "FileName;ErrorMessage;");
    }
}