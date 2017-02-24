using System;
using System.Collections.Concurrent;
using System.Text;
using EvilBaschdi.Core.Logging;
using Folders2Md5.Models;

namespace Folders2Md5.Internal
{
    /// <summary>
    ///     Is generating a logging file.
    /// </summary>
    public class Logging : ILogging
    {
        /// <inheritdoc />
        public void Run(ConcurrentBag<Folders2Md5LogEntry> folders2Md5LogEntries, Configuration configuration)
        {
            if (folders2Md5LogEntries == null)
            {
                throw new ArgumentNullException(nameof(folders2Md5LogEntries));
            }
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            var appendAllTextWithHeadline = new AppendAllTextWithHeadline();
            var stringBuilder = new StringBuilder();

            foreach (var logEntry in folders2Md5LogEntries)
            {
                if (logEntry != null)
                {
                    stringBuilder.Append(
                        $"{logEntry.FileName};{logEntry.Type};{logEntry.HashSum};{logEntry.AlreadyExisting};{Environment.NewLine}");
                }
            }

            appendAllTextWithHeadline.For($@"{configuration.LoggingPath}\Folders2Md5_Log_{DateTime.Now:yyyy-MM-dd_HHmm}.csv", stringBuilder,
                "FileName;Type;HashSum;AlreadyExisting;");
        }
    }
}