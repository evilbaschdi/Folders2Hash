using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using EvilBaschdi.Core.Logging;
using Folders2Hash.Models;

namespace Folders2Hash.Internal
{
    /// <summary>
    ///     Is generating a logging file.
    /// </summary>
    public class Logging : ILogging
    {
        /// <inheritdoc />
        public void Run(ConcurrentBag<LogEntry> logEntries, Configuration configuration)
        {
            if (logEntries == null)
            {
                throw new ArgumentNullException(nameof(logEntries));
            }
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var appendAllTextWithHeadline = new AppendAllTextWithHeadline();
            var stringBuilder = new StringBuilder();

            foreach (var logEntry in logEntries.OrderBy(x => x.FileName).ThenBy(x => x.Type))
            {
                if (logEntry != null)
                {
                    stringBuilder.Append(
                        $"{logEntry.FileName};{logEntry.Type};{logEntry.HashSum};{logEntry.AlreadyExisting};{Environment.NewLine}");
                }
            }

            appendAllTextWithHeadline.For($@"{configuration.LoggingPath}\Folders2Hash_Log_{DateTime.Now:yyyy-MM-dd_HHmm}.csv", stringBuilder,
                "FileName;Type;HashSum;AlreadyExisting;");
        }
    }
}